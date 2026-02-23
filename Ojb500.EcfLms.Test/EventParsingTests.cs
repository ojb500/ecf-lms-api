using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class EventParsingTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        private static Event[] ParseEvents(string json)
            => JsonSerializer.Deserialize<ApiResult<Event>[]>(json)[0].Data;

        [TestMethod]
        public void ParseEventResponse()
        {
            var json = ReadSample("event-excerpt.json");
            var results = JsonSerializer.Deserialize<ApiResult<Event>[]>(json);

            Assert.AreEqual(1, results.Length);
            var result = results[0];
            Assert.AreEqual("Fixtures for Div 1 - Davy Trophy", result.Title);
            Assert.AreEqual(6, result.Header.Length);
            Assert.AreEqual("Home Team", result.Header[0]);
            Assert.AreEqual(4, result.Data.Length);
        }

        [TestMethod]
        public void ParseEventScores()
        {
            var data = ParseEvents(ReadSample("event-excerpt.json"));

            // "1 - 5"
            Assert.AreEqual(1, data[0].Result.Home.PointsX2 / 2);
            Assert.AreEqual(5, data[0].Result.Away.PointsX2 / 2);
            Assert.IsFalse(data[0].Result.IsEmpty);

            // "3 - 3" (draw)
            Assert.AreEqual(3, data[1].Result.Home.PointsX2 / 2);
            Assert.AreEqual(3, data[1].Result.Away.PointsX2 / 2);
            Assert.IsFalse(data[1].Result.IsEmpty);

            // "0 - 0" (unplayed)
            Assert.IsTrue(data[3].Result.IsEmpty);
        }

        [TestMethod]
        public void ParseEventHalfPointScores()
        {
            // The API can return half-point scores like "4½ - 1½"
            var json = @"[{""title"":""Test"",""header"":[""Home Team"",""Result"",""Away Team"",""Date"",""Time"",""Status""],
                ""data"":[[""Team A"",""4½ - 1½"",""Team B"",""Mon 29 Sep 25"",""19:30"",""OU""]]}]";
            var data = ParseEvents(json);

            Assert.AreEqual(9, data[0].Result.Home.PointsX2);  // 4½ × 2 = 9
            Assert.AreEqual(3, data[0].Result.Away.PointsX2);   // 1½ × 2 = 3
            Assert.AreEqual("4½", data[0].Result.Home.ToString());
            Assert.AreEqual("1½", data[0].Result.Away.ToString());
        }

        [TestMethod]
        public void ParseEventTeamNames()
        {
            var data = ParseEvents(ReadSample("event-excerpt.json"));

            Assert.AreEqual("Darnall & Handsworth A", data[0].Home.Name);
            Assert.AreEqual("Woodseats A", data[0].Away.Name);
            Assert.AreEqual("Hillsborough A", data[1].Home.Name);
            Assert.AreEqual("Sheffield Nomads A", data[1].Away.Name);
        }

        [TestMethod]
        public void ParseEventDateFormats()
        {
            var data = ParseEvents(ReadSample("event-excerpt.json"));

            // "Mon 29 Sep 25" at 19:30
            Assert.AreEqual(new DateTime(2025, 9, 29, 19, 30, 0), data[0].DateTime);

            // "Wed 1 Oct 25" — single digit day
            Assert.AreEqual(new DateTime(2025, 10, 1, 19, 30, 0), data[2].DateTime);

            // "Mon 23 Feb 26"
            Assert.AreEqual(new DateTime(2026, 2, 23, 19, 30, 0), data[3].DateTime);
        }

        [TestMethod]
        public void ParseEventPostponedDate()
        {
            var json = @"[{""title"":""Test"",""header"":[""H"",""R"",""A"",""D"",""T"",""S""],
                ""data"":[[""Team A"",""0 - 0"",""Team B"",""Postponed"",""19:30"",""OU""]]}]";
            var data = ParseEvents(json);
            Assert.IsNull(data[0].DateTime);
        }
    }
}
