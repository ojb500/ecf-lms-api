using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class MatchParsingTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        private static MatchCard[] ParseMatchCards(string json)
            => JsonSerializer.Deserialize<MatchApiResult[]>(json)
                .Select(Api.ParseMatchCard)
                .ToArray();

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
        public void ParseMatchResponse_Ratings()
        {
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[0];

            // "2081 (2031)" → Primary=2081, Secondary=2031
            Assert.AreEqual(2081, mc.Pairings[0].FirstPlayer.Rating.Primary);
            Assert.AreEqual(2031, mc.Pairings[0].FirstPlayer.Rating.Secondary);

            // "2160 (2191)"
            Assert.AreEqual(2160, mc.Pairings[0].SecondPlayer.Rating.Primary);
            Assert.AreEqual(2191, mc.Pairings[0].SecondPlayer.Rating.Secondary);

            // "1835 (1818)"
            Assert.AreEqual(1835, mc.Pairings[5].FirstPlayer.Rating.Primary);
            Assert.AreEqual(1818, mc.Pairings[5].FirstPlayer.Rating.Secondary);
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
            Assert.AreEqual(0, defaultBoard.SecondPlayer.Rating.Primary);
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
        public void ParseMatchResponse_UnratedRating()
        {
            // Match 5: board 6 has "0000 (1751)" → Primary=0, Secondary=1751
            var mc = ParseMatchCards(ReadSample("match-excerpt.json"))[4];
            var board6 = mc.Pairings[5];
            Assert.AreEqual("King", board6.FirstPlayer.FamilyName);
            Assert.AreEqual(0, board6.FirstPlayer.Rating.Primary);
            Assert.AreEqual(1751, board6.FirstPlayer.Rating.Secondary);
        }
    }
}
