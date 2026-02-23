using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class TableParsingTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        private static ApiResult<LeagueTableEntry> ParseTable(string json)
            => JsonSerializer.Deserialize<ApiResult<LeagueTableEntry>[]>(json)[0];

        [TestMethod]
        public void ParseTableResponse()
        {
            var table = ParseTable(ReadSample("table-div1.json"));

            Assert.AreEqual("Div 1 - Davy Trophy", table.Title);
            Assert.AreEqual(8, table.Data.Length);
        }

        [TestMethod]
        public void ParseTableResponse_TeamNames()
        {
            var table = ParseTable(ReadSample("table-div1.json"));

            Assert.AreEqual("Worksop A", table.Data[0].Team.Name);
            Assert.AreEqual("Woodseats A", table.Data[1].Team.Name);
            Assert.AreEqual("Darnall & Handsworth A", table.Data[7].Team.Name);
        }

        [TestMethod]
        public void ParseTableResponse_Record()
        {
            var table = ParseTable(ReadSample("table-div1.json"));

            // Worksop A: P10, W7, D1, L2
            var worksop = table.Data[0];
            Assert.AreEqual(10, worksop.P);
            Assert.AreEqual(7, worksop.W);
            Assert.AreEqual(1, worksop.D);
            Assert.AreEqual(2, worksop.L);
        }

        [TestMethod]
        public void ParseTableResponse_ForAndAgainst()
        {
            var table = ParseTable(ReadSample("table-div1.json"));

            // Worksop A: F=35, A=24 (whole numbers)
            Assert.AreEqual(70, table.Data[0].F.PointsX2);   // 35 × 2
            Assert.AreEqual(48, table.Data[0].A.PointsX2);   // 24 × 2

            // Woodseats A: F=29½, A=18½ (half-point values)
            Assert.AreEqual(59, table.Data[1].F.PointsX2);   // 29½ × 2
            Assert.AreEqual(37, table.Data[1].A.PointsX2);   // 18½ × 2
        }

        [TestMethod]
        public void ParseTableAsLeagueTable()
        {
            var json = ReadSample("table-div1.json");
            var results = JsonSerializer.Deserialize<ApiResult<LeagueTableEntry>[]>(json);
            Assert.AreEqual(1, results.Length);
            var r = results[0];
            var table = new LeagueTable { Title = r.Title, Header = r.Header, Data = r.Data };

            Assert.AreEqual("Div 1 - Davy Trophy", table.Name);
            Assert.AreEqual(8, table.Data.Length);

            // Verify IEnumerable<LeaguePosition> works
            var positions = table.ToList();
            Assert.AreEqual(8, positions.Count);
            Assert.AreEqual(1, positions[0].Position);
            Assert.AreEqual("Worksop A", positions[0].Entry.Team.Name);
        }

        [TestMethod]
        public void ParseTableResponse_Points()
        {
            var table = ParseTable(ReadSample("table-div1.json"));

            // Worksop A: 15 pts
            Assert.AreEqual(30, table.Data[0].Pts.PointsX2);   // 15 × 2

            // Woodseats A: 12 pts
            Assert.AreEqual(24, table.Data[1].Pts.PointsX2);   // 12 × 2

            // Darnall & Handsworth A: 3 pts
            Assert.AreEqual(6, table.Data[7].Pts.PointsX2);    // 3 × 2
        }
    }
}
