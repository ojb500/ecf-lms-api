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
        private static readonly Dictionary<string, string> PathToFile = new Dictionary<string, string>
        {
            ["table"] = "table-div1.json",
            ["event"] = "event-excerpt.json",
            ["match"] = "match-excerpt.json",
            ["club"] = "club-rotherham.json",
            ["seasons"] = "seasons.json",
            ["seasonsWithEvents"] = "seasonsWithEvents.json",
            ["standings"] = "standings.json",
        };

        private static readonly Dictionary<(string, string), string> NamedPathToFile = new Dictionary<(string, string), string>
        {
            [("table", "Richardson Cup")] = "table-cup.json",
            [("event", "Richardson Cup")] = "event-cup.json",
            [("match", "Richardson Cup")] = "match-cup.json",
        };

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
                NamedPathToFile.TryGetValue((path, name), out file);
            if (file == null && !PathToFile.TryGetValue(path, out file))
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
        private Api _api;

        [TestInitialize]
        public void Setup()
        {
            _handler = new SampleHttpHandler();
            _api = new Api(_handler);
        }

        [TestMethod]
        public async Task GetTableAsync_ReturnsLeagueTable()
        {
            IModel model = _api;
            var table = await model.GetTableAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(table);
            Assert.AreEqual("Div 1 - Davy Trophy", table.Name);
            Assert.AreEqual(8, table.Data.Length);
            Assert.AreEqual("Worksop A", table.Data[0].Team.Name);
        }

        [TestMethod]
        public async Task GetEventsAsync_ReturnsEvents()
        {
            IModel model = _api;
            var events = await model.GetEventsAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(events);
            Assert.AreEqual(4, events.Length);
            Assert.AreEqual("Darnall & Handsworth A", events[0].Home.Name);
            Assert.AreEqual("Woodseats A", events[0].Away.Name);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_ReturnsMatchCards()
        {
            IModel model = _api;
            var matches = await model.GetMatchCardsAsync("613", "Div 1 - Davy Trophy");

            Assert.IsNotNull(matches);
            Assert.AreEqual(5, matches.Length);
            Assert.AreEqual("Chesterfield A", matches[0].Left.Name);
            Assert.AreEqual("Ecclesall A", matches[0].Right.Name);
            Assert.AreEqual(6, matches[0].Pairings.Length);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_ParsesAdjustments()
        {
            IModel model = _api;
            var matches = await model.GetMatchCardsAsync("613", "Div 1 - Davy Trophy");

            // The last match (Ecclesall B v SASCA B) has an adjustment row
            var lastMatch = matches[4];
            Assert.AreEqual(1, lastMatch.Adjustments.Length);
        }

        [TestMethod]
        public async Task GetClubEventsAsync_ReturnsCompetitionEvents()
        {
            IModel model = _api;
            var clubEvents = await model.GetClubEventsAsync("613", "rotherham");

            Assert.IsNotNull(clubEvents);
            Assert.AreEqual(1, clubEvents.Length);
            Assert.IsTrue(clubEvents[0].Title.Contains("Rotherham"));
            Assert.IsTrue(clubEvents[0].Events.Length > 0);
        }

        [TestMethod]
        public async Task GetSeasonsAsync_ReturnsDictionary()
        {
            IModel model = _api;
            var seasons = await model.GetSeasonsAsync("613");

            Assert.IsNotNull(seasons);
            Assert.IsTrue(seasons.Count > 0);
            Assert.IsTrue(seasons.ContainsValue("2025-26"));
        }

        [TestMethod]
        public async Task GetSeasonsWithEventsAsync_ReturnsDictionary()
        {
            IModel model = _api;
            var seasons = await model.GetSeasonsWithEventsAsync("613");

            Assert.IsNotNull(seasons);
            Assert.IsTrue(seasons.Count > 0);
            var first = seasons.Values.First(s => s.Name == "2025-26");
            Assert.IsTrue(first.Events.Count > 0);
        }

        [TestMethod]
        public async Task CancellationToken_IsPropagated()
        {
            IModel model = _api;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(
                () => model.GetTableAsync("613", "test", cts.Token));
        }

        [TestMethod]
        public async Task Organisation_Competition_Chain()
        {
            IModel model = _api;
            var org = model.GetOrganisation(613);
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
            IModel model = _api;
            var org = model.GetOrganisation(613);
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
            IModel model = _api;

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => model.GetTableAsync("613", "Richardson Cup"));

            StringAssert.Contains(ex.Message, "Richardson Cup");
            StringAssert.Contains(ex.Message, "cup");
        }

        [TestMethod]
        public async Task GetEventsAsync_Cup_ReturnsFixtures()
        {
            IModel model = _api;
            var events = await model.GetEventsAsync("613", "Richardson Cup");

            Assert.IsNotNull(events);
            Assert.AreEqual(5, events.Length);
            Assert.AreEqual("SASCA 1", events[0].Home.Name);
            Assert.AreEqual("Sheffield Deaf", events[0].Away.Name);
        }

        [TestMethod]
        public async Task GetEventsAsync_Cup_ByeFixtureHasDefaultAwayTeam()
        {
            IModel model = _api;
            var events = await model.GetEventsAsync("613", "Richardson Cup");

            // The bye fixture has null away team in the JSON
            var bye = events[3];
            Assert.AreEqual("Sheffield Nomads 1", bye.Home.Name);
            Assert.IsNull(bye.Away.Name);
        }

        [TestMethod]
        public async Task GetMatchCardsAsync_Cup_ReturnsMatchCards()
        {
            IModel model = _api;
            var matches = await model.GetMatchCardsAsync("613", "Richardson Cup");

            Assert.IsNotNull(matches);
            Assert.AreEqual(2, matches.Length);
            Assert.AreEqual("SASCA 1", matches[0].Left.Name);
            Assert.AreEqual("Sheffield Deaf", matches[0].Right.Name);
            Assert.AreEqual(6, matches[0].Pairings.Length);
        }

        [TestMethod]
        public async Task Competition_Cup_EventsAndMatchesWork_TableThrows()
        {
            IModel model = _api;
            var org = model.GetOrganisation(613);
            var cup = org.GetCompetition("Richardson Cup");

            var events = await cup.GetEventsAsync();
            Assert.AreEqual(5, events.Length);

            var matches = await cup.GetMatchesAsync();
            Assert.AreEqual(2, matches.Length);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => cup.GetTableAsync());
        }
    }
}
