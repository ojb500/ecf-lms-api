using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class SampleDataTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        private static Event[] ParseEvents(string json)
            => JsonSerializer.Deserialize<ApiResult<Event>[]>(json)[0].Data;

        private static MatchCard[] ParseMatchCards(string json)
            => JsonSerializer.Deserialize<MatchApiResult[]>(json)
                .Select(Api.ParseMatchCard)
                .ToArray();

        #region Event parsing

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

        #endregion

        #region Match parsing

        [TestMethod]
        public void ParseMatchResponse_NormalPairings()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            Assert.AreEqual("Chesterfield A", mc.Left.Name);
            Assert.AreEqual("Ecclesall A", mc.Right.Name);
            Assert.AreEqual(6, mc.Pairings.Length);
            Assert.AreEqual(0, mc.Adjustments.Length);
        }

        [TestMethod]
        public void ParseMatchResponse_BoardNumberAndColour()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            // Board 1 (B) → board=1, white=false
            Assert.AreEqual(1, mc.Pairings[0].Board);
            Assert.IsFalse(mc.Pairings[0].FirstPlayerWhite);

            // Board 2 (W) → board=2, white=true
            Assert.AreEqual(2, mc.Pairings[1].Board);
            Assert.IsTrue(mc.Pairings[1].FirstPlayerWhite);

            // Board 6 (W) → board=6, white=true
            Assert.AreEqual(6, mc.Pairings[5].Board);
            Assert.IsTrue(mc.Pairings[5].FirstPlayerWhite);
        }

        [TestMethod]
        public void ParseMatchResponse_PlayerNames()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            // "Ackley, Peter"
            Assert.AreEqual("Ackley", mc.Pairings[0].FirstPlayer.FamilyName);
            Assert.AreEqual("Peter", mc.Pairings[0].FirstPlayer.GivenName);

            // "Starley, Robert"
            Assert.AreEqual("Starley", mc.Pairings[0].SecondPlayer.FamilyName);
            Assert.AreEqual("Robert", mc.Pairings[0].SecondPlayer.GivenName);

            // "Sullivan, Daniel JS" — given name includes middle initials
            Assert.AreEqual("Sullivan", mc.Pairings[2].SecondPlayer.FamilyName);
            Assert.AreEqual("Daniel JS", mc.Pairings[2].SecondPlayer.GivenName);
        }

        [TestMethod]
        public void ParseMatchResponse_Grades()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            // "2081 (2031)" → current rating: 2081
            Assert.AreEqual(2081, mc.Pairings[0].FirstPlayer.Grade.Value);

            // "2160 (2191)"
            Assert.AreEqual(2160, mc.Pairings[0].SecondPlayer.Grade.Value);

            // "1835 (1818)"
            Assert.AreEqual(1835, mc.Pairings[5].FirstPlayer.Grade.Value);
        }

        [TestMethod]
        public void ParseMatchResponse_IndividualResults()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            // Board 1: "1 - 0" → LeftWin
            Assert.AreEqual(Result.LeftWin, mc.Pairings[0].Result.Result);
            Assert.IsFalse(mc.Pairings[0].Result.WasDefaulted);

            // Board 2: "½ - ½" → Draw
            Assert.AreEqual(Result.Draw, mc.Pairings[1].Result.Result);

            // Board 3: "½ - ½" → Draw
            Assert.AreEqual(Result.Draw, mc.Pairings[2].Result.Result);
        }

        [TestMethod]
        public void ParseMatchResponse_DefaultEntry()
        {
            // Match 2: Sheffield Nomads A v Ecclesall A — board 6 has Default
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[1];

            var defaultBoard = mc.Pairings[5];
            Assert.AreEqual(6, defaultBoard.Board);
            Assert.AreEqual("Hughes", defaultBoard.FirstPlayer.FamilyName);
            Assert.IsTrue(defaultBoard.SecondPlayer.IsDefault);
            Assert.AreEqual("Default", defaultBoard.SecondPlayer.FamilyName);
            Assert.AreEqual(0, defaultBoard.SecondPlayer.Grade.Value);
        }

        [TestMethod]
        public void ParseMatchResponse_UnplayedBoard()
        {
            // Match 3: Hillsborough A v Worksop A — board 1 has "0 - 0"
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[2];
            Assert.AreEqual(Result.Unplayed, mc.Pairings[0].Result.Result);
        }

        [TestMethod]
        public void ParseMatchResponse_DefaultWin()
        {
            // Match 4: Worksop A v Hillsborough A — board 2 has "1 - 0(def)"
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[3];

            var defWin = mc.Pairings[1];
            Assert.AreEqual(2, defWin.Board);
            Assert.AreEqual(Result.LeftWin, defWin.Result.Result);
            Assert.IsTrue(defWin.Result.WasDefaulted);
        }

        [TestMethod]
        public void ParseMatchResponse_AdjustmentRow()
        {
            // Match 5: Ecclesall B v SASCA B — has Adjustment "3 : -3"
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[4];

            Assert.AreEqual("Ecclesall B", mc.Left.Name);
            Assert.AreEqual("SASCA B", mc.Right.Name);
            Assert.AreEqual(6, mc.Pairings.Length);
            Assert.AreEqual(1, mc.Adjustments.Length);

            var adj = mc.Adjustments[0];
            Assert.AreEqual(6, adj.Score.Home.PointsX2);   // 3 × 2 = 6
            Assert.AreEqual(-6, adj.Score.Away.PointsX2);   // -3 × 2 = -6
            Assert.AreEqual("3", adj.Score.Home.ToString());
            Assert.AreEqual("-3", adj.Score.Away.ToString());
        }

        [TestMethod]
        public void ParseMatchResponse_UnratedGrade()
        {
            // Match 5: board 6 has "0000 (1751)" → no current rating → grade=0
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[4];
            var board6 = mc.Pairings[5];
            Assert.AreEqual("King", board6.FirstPlayer.FamilyName);
            Assert.AreEqual(0, board6.FirstPlayer.Grade.Value);
        }

        #endregion

        #region Score/Points unit tests

        [TestMethod]
        public void PointsParsesNegative()
        {
            Assert.IsTrue(Points.TryParse("-3".AsSpan(), out var pts, out _));
            Assert.AreEqual(-6, pts.PointsX2);
            Assert.AreEqual("-3", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesNegativeHalf()
        {
            Assert.IsTrue(Points.TryParse("-½".AsSpan(), out var pts, out _));
            Assert.AreEqual(-1, pts.PointsX2);
            Assert.AreEqual("-½", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesHalf()
        {
            Assert.IsTrue(Points.TryParse("½".AsSpan(), out var pts, out _));
            Assert.AreEqual(1, pts.PointsX2);
            Assert.AreEqual("½", pts.ToString());
        }

        [TestMethod]
        public void PointsParsesIntegerWithHalf()
        {
            Assert.IsTrue(Points.TryParse("4½".AsSpan(), out var pts, out _));
            Assert.AreEqual(9, pts.PointsX2);
            Assert.AreEqual("4½", pts.ToString());
        }

        [TestMethod]
        public void ScoreFromStandardFormat()
        {
            var s = new Score("3 - 2");
            Assert.AreEqual(6, s.Home.PointsX2);
            Assert.AreEqual(4, s.Away.PointsX2);
        }

        [TestMethod]
        public void ScoreFromHalfPoints()
        {
            var s = new Score("4½ - 1½");
            Assert.AreEqual(9, s.Home.PointsX2);
            Assert.AreEqual(3, s.Away.PointsX2);
        }

        #endregion
    }
}
