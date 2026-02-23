using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Ojb500.EcfLms.Test
{
    internal class SampleHttpHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _defaults = new();
        private readonly Dictionary<(string, string), string> _named = new();

        public void Map(string endpoint, string sampleFile)
            => _defaults[endpoint] = sampleFile;

        public void Map(string endpoint, string name, string sampleFile)
            => _named[(endpoint, name)] = sampleFile;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri.AbsolutePath.Split('/').Last();

            string name = null;
            if (request.Content != null)
            {
                var body = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("name", out var nameProp) &&
                    nameProp.ValueKind == JsonValueKind.String)
                {
                    name = nameProp.GetString();
                }
            }

            string file = null;
            if (name != null)
                _named.TryGetValue((path, name), out file);
            if (file == null && !_defaults.TryGetValue(path, out file))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(Path.Combine("Samples", file));
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return response;
        }
    }

    [TestClass]
    public class ApiTests
    {
        private SampleHttpHandler _handler;
        private IModel _api;

        [TestInitialize]
        public void Setup()
        {
            _handler = new SampleHttpHandler();
            _api = new Api(_handler);
        }

        [TestMethod]
        public async Task GetTableAsync_ReturnsLeagueTable()
        {
            _handler.Map("table", "table-div1.json");

            var table = await _api.GetTableAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(table);
            Assert.AreEqual("Div 1 - Davy Trophy", table.Name);
            Assert.AreEqual(8, table.Data.Length);
            Assert.AreEqual("Worksop A", table.Data[0].Team.Name);
        }

        [TestMethod]
        public async Task GetEventsAsync_ReturnsEvents()
        {
            _handler.Map("event", "event-excerpt.json");

            var events = await _api.GetEventsAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(events);
            Assert.AreEqual(4, events.Length);
            Assert.AreEqual("Darnall & Handsworth A", events[0].Home.Name);
            Assert.AreEqual("Woodseats A", events[0].Away.Name);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_ReturnsMatchCards()
        {
            _handler.Map("match", "match-excerpt.json");

            var matches = await _api.GetMatchCardsAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(matches);
            Assert.AreEqual(5, matches.Length);
            Assert.AreEqual("Chesterfield A", matches[0].Left.Name);
            Assert.AreEqual("Ecclesall A", matches[0].Right.Name);
            Assert.AreEqual(6, matches[0].Pairings.Length);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_ParsesAdjustments()
        {
            _handler.Map("match", "match-excerpt.json");

            var matches = await _api.GetMatchCardsAsync("613", "Div 1 - Davy Trophy");

            // The last match (Ecclesall B v SASCA B) has an adjustment row
            var lastMatch = matches[4];
            Assert.AreEqual(1, lastMatch.Adjustments.Length);
        }

        [TestMethod]
        public async Task GetClubEventsAsync_ReturnsCompetitionEvents()
        {
            _handler.Map("club", "club-rotherham.json");

            var clubEvents = await _api.GetClubEventsAsync("613", "rotherham");

            Assert.IsNotNull(clubEvents);
            Assert.AreEqual(1, clubEvents.Length);
            Assert.IsTrue(clubEvents[0].Title.Contains("Rotherham"));
            Assert.IsTrue(clubEvents[0].Events.Length > 0);
        }

        [TestMethod]
        public async Task GetSeasonsAsync_ReturnsDictionary()
        {
            _handler.Map("seasons", "seasons.json");

            var seasons = await _api.GetSeasonsAsync("613");

            Assert.IsNotNull(seasons);
            Assert.IsTrue(seasons.Count > 0);
            Assert.IsTrue(seasons.ContainsValue("2025-26"));
        }

        [TestMethod]
        public async Task GetSeasonsWithEventsAsync_ReturnsDictionary()
        {
            _handler.Map("seasonsWithEvents", "seasonsWithEvents.json");

            var seasons = await _api.GetSeasonsWithEventsAsync("613");

            Assert.IsNotNull(seasons);
            Assert.IsTrue(seasons.Count > 0);
            var first = seasons.Values.First(s => s.Name == "2025-26");
            Assert.IsTrue(first.Events.Count > 0);
        }

        [TestMethod]
        public async Task CancellationToken_IsPropagated()
        {

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => _api.GetTableAsync("613", "test", cts.Token));
        }

        [TestMethod]
        public async Task Organisation_Competition_Chain()
        {
            _handler.Map("event", "event-excerpt.json");
            _handler.Map("table", "table-div1.json");
            _handler.Map("match", "match-excerpt.json");

            var org = _api.GetOrganisation(613);
            var comp = org.GetCompetition("Div 1 - Davy Trophy");

            var events = await comp.GetEventsAsync();
            Assert.AreEqual(4, events.Length);

            var table = await comp.GetTableAsync();
            Assert.AreEqual("Div 1 - Davy Trophy", table.Name);
            Assert.AreEqual(comp, table.Competition);

            var matches = await comp.GetMatchesAsync();
            Assert.AreEqual(5, matches.Length);
        }

        [TestMethod]
        public async Task Competition_CachesResults()
        {
            _handler.Map("event", "event-excerpt.json");
            _handler.Map("table", "table-div1.json");
            _handler.Map("match", "match-excerpt.json");

            var org = _api.GetOrganisation(613);
            var comp = org.GetCompetition("Div 1 - Davy Trophy");

            var events1 = await comp.GetEventsAsync();
            var events2 = await comp.GetEventsAsync();
            Assert.AreSame(events1, events2);

            var matches1 = await comp.GetMatchesAsync();
            var matches2 = await comp.GetMatchesAsync();
            Assert.AreSame(matches1, matches2);

            var table1 = await comp.GetTableAsync();
            var table2 = await comp.GetTableAsync();
            Assert.AreSame(table1, table2);
        }

        // --- Cup competition tests ---

        [TestMethod]
        public async Task GetTableAsync_Cup_ThrowsMeaningfulException()
        {
            _handler.Map("table", "Richardson Cup", "table-cup.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetTableAsync("613", "Richardson Cup"));

            StringAssert.Contains(ex.Message, "Richardson Cup");
            StringAssert.Contains(ex.Message, "cup");
        }

        [TestMethod]
        public async Task GetEventsAsync_Cup_ReturnsFixtures()
        {
            _handler.Map("event", "Richardson Cup", "event-cup.json");

            var events = await _api.GetEventsAsync("613", "Richardson Cup");

            Assert.IsNotNull(events);
            Assert.AreEqual(5, events.Length);
            Assert.AreEqual("SASCA 1", events[0].Home.Name);
            Assert.AreEqual("Sheffield Deaf", events[0].Away.Name);
        }

        [TestMethod]
        public async Task GetEventsAsync_Cup_ByeFixtureHasDefaultAwayTeam()
        {
            _handler.Map("event", "Richardson Cup", "event-cup.json");

            var events = await _api.GetEventsAsync("613", "Richardson Cup");

            // The bye fixture has null away team in the JSON
            var bye = events[3];
            Assert.AreEqual("Sheffield Nomads 1", bye.Home.Name);
            Assert.IsNull(bye.Away.Name);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_Cup_ReturnsMatchCards()
        {
            _handler.Map("match", "Richardson Cup", "match-cup.json");

            var matches = await _api.GetMatchCardsAsync("613", "Richardson Cup");

            Assert.IsNotNull(matches);
            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("SASCA 1", matches[0].Left.Name);
            Assert.AreEqual("Sheffield Deaf", matches[0].Right.Name);
            Assert.AreEqual(6, matches[0].Pairings.Length);
        }

        [TestMethod]
        public async Task Competition_Cup_EventsAndMatchesWork_TableThrows()
        {
            _handler.Map("event", "Richardson Cup", "event-cup.json");
            _handler.Map("match", "Richardson Cup", "match-cup.json");
            _handler.Map("table", "Richardson Cup", "table-cup.json");

            var org = _api.GetOrganisation(613);
            var cup = org.GetCompetition("Richardson Cup");

            var events = await cup.GetEventsAsync();
            Assert.AreEqual(5, events.Length);

            var matches = await cup.GetMatchesAsync();
            Assert.AreEqual(2, matches.Length);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => cup.GetTableAsync());
        }

        // --- Crosstable tests ---

        [TestMethod]
        public async Task GetCrosstableAsync_ReturnsCrosstable()
        {
            _handler.Map("standings", "standings.json");

            var crosstable = await _api.GetCrosstableAsync("613", "Sheffield Individual Championships");

            Assert.IsNotNull(crosstable);
            Assert.AreEqual("Sheffield Individual Championships", crosstable.Title);
            Assert.AreEqual(34, crosstable.Entries.Length);
            Assert.AreEqual(4, crosstable.Rounds.Length);
            Assert.AreEqual("Round 1", crosstable.Rounds[0]);
            Assert.AreEqual("Round 4", crosstable.Rounds[3]);
        }

        [TestMethod]
        public async Task GetCrosstableAsync_ParsesRoundResults()
        {
            _handler.Map("standings", "standings.json");

            var crosstable = await _api.GetCrosstableAsync("613", "Sheffield Individual Championships");

            // Entry 0: "Alexandr Klimchik", "1 (b22)", "1 (w13)", "1 (b12)", "- (  )"
            var entry = crosstable.Entries[0];
            Assert.AreEqual(1, entry.SeedNumber);
            Assert.AreEqual("Alexandr Klimchik", entry.Name);

            // Round 1: "1 (b22)" - win with black against 22
            Assert.AreEqual(2, entry.Results[0].Score.PointsX2); // 1 point
            Assert.AreEqual(false, entry.Results[0].IsWhite);
            Assert.AreEqual(22, entry.Results[0].OpponentNumber);
            Assert.IsTrue(entry.Results[0].IsPlayed);

            // Round 2: "1 (w13)" - win with white against 13
            Assert.AreEqual(2, entry.Results[1].Score.PointsX2);
            Assert.AreEqual(true, entry.Results[1].IsWhite);
            Assert.AreEqual(13, entry.Results[1].OpponentNumber);

            // Look up opponent by seed number via indexer
            var opponent = crosstable[entry.Results[0].OpponentNumber];
            Assert.AreEqual(22, opponent.SeedNumber);
            Assert.IsNotNull(opponent.Name);
        }

        [TestMethod]
        public async Task GetCrosstableAsync_ParsesSpecialResults()
        {
            _handler.Map("standings", "standings.json");

            var crosstable = await _api.GetCrosstableAsync("613", "Sheffield Individual Championships");

            // Entry 16 (Colin Reid): "1 ( def)", "0 (b2)", " ½ ( HPB)", "- (  )"
            var entry = crosstable.Entries[16];
            Assert.AreEqual("Colin Reid", entry.Name);

            // Round 1: default win — no colour
            Assert.IsTrue(entry.Results[0].IsDefault);
            Assert.IsTrue(entry.Results[0].IsPlayed);
            Assert.IsNull(entry.Results[0].IsWhite);
            Assert.AreEqual(2, entry.Results[0].Score.PointsX2);

            // Round 3: half point bye — no colour
            Assert.IsTrue(entry.Results[2].IsHalfPointBye);
            Assert.IsTrue(entry.Results[2].IsPlayed);
            Assert.IsNull(entry.Results[2].IsWhite);
            Assert.AreEqual(1, entry.Results[2].Score.PointsX2);

            // Round 4: unplayed — no colour
            Assert.IsFalse(entry.Results[3].IsPlayed);
            Assert.IsNull(entry.Results[3].IsWhite);
        }

        [TestMethod]
        public async Task GetCrosstableAsync_ParsesTotal()
        {
            _handler.Map("standings", "standings.json");

            var crosstable = await _api.GetCrosstableAsync("613", "Sheffield Individual Championships");

            // Entry 0: total 3 (integer)
            Assert.AreEqual(6, crosstable.Entries[0].Total.PointsX2);

            // Entry 3 (Jim Davis): total "2½"
            Assert.AreEqual(5, crosstable.Entries[3].Total.PointsX2);

            // Entry 29 (Craig Fores): total " ½"
            Assert.AreEqual(1, crosstable.Entries[29].Total.PointsX2);
        }

        [TestMethod]
        public async Task GetCrosstableAsync_League_ThrowsMeaningfulException()
        {
            _handler.Map("standings", "Div 1 - Davy Trophy", "standings-error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetCrosstableAsync("613", "Div 1 - Davy Trophy"));

            StringAssert.Contains(ex.Message, "ERROR:");
            StringAssert.Contains(ex.Message, "team event");
        }

        [TestMethod]
        public async Task GetCrosstableAsync_Cup_ThrowsMeaningfulException()
        {
            _handler.Map("standings", "Richardson Cup", "standings-error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetCrosstableAsync("613", "Richardson Cup"));

            StringAssert.Contains(ex.Message, "ERROR:");
            StringAssert.Contains(ex.Message, "team event");
        }

        [TestMethod]
        public async Task GetCrosstableAsync_Nonexistent_ThrowsMeaningfulException()
        {
            _handler.Map("standings", "Nonexistent", "error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetCrosstableAsync("613", "Nonexistent"));

            StringAssert.Contains(ex.Message, "ERROR:");
        }

        // --- Nonexistent competition error tests ---

        [TestMethod]
        public async Task GetTableAsync_Nonexistent_ThrowsMeaningfulException()
        {
            _handler.Map("table", "Nonexistent", "error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetTableAsync("613", "Nonexistent"));

            StringAssert.Contains(ex.Message, "ERROR:");
        }

        [TestMethod]
        public async Task GetEventsAsync_Nonexistent_ThrowsMeaningfulException()
        {
            _handler.Map("event", "Nonexistent", "error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetEventsAsync("613", "Nonexistent"));

            StringAssert.Contains(ex.Message, "ERROR:");
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_Nonexistent_ThrowsMeaningfulException()
        {
            _handler.Map("match", "Nonexistent", "error.json");


            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _api.GetMatchCardsAsync("613", "Nonexistent"));

            StringAssert.Contains(ex.Message, "ERROR:");
        }

        [TestMethod]
        public async Task Competition_GetCrosstableAsync()
        {
            _handler.Map("standings", "standings.json");

            var org = _api.GetOrganisation(613);
            var comp = org.GetCompetition("Sheffield Individual Championships");

            var ct1 = await comp.GetCrosstableAsync();
            Assert.IsNotNull(ct1);
            Assert.AreEqual(comp, ct1.Competition);
            Assert.AreEqual("Sheffield Individual Championships", ct1.Name);

            // Verify caching
            var ct2 = await comp.GetCrosstableAsync();
            Assert.AreSame(ct1, ct2);
        }
    }
}

