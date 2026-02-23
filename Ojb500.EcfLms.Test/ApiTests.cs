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

        public CancellationToken LastCancellationToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastCancellationToken = cancellationToken;

            var path = request.RequestUri.AbsolutePath.Split('/').Last();
            if (!PathToFile.TryGetValue(path, out var file))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            var json = File.ReadAllText(Path.Combine("Samples", file));
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
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
    }
}
