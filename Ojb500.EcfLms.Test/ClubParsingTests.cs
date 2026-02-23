using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class ClubParsingTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        [TestMethod]
        public void ParseClubResponse()
        {
            var json = ReadSample("club-rotherham.json");
            var results = JsonSerializer.Deserialize<ApiResult<Event>[]>(json);

            Assert.AreEqual(1, results.Length);
            var result = results[0];
            Assert.IsTrue(result.Title.Contains("Rotherham"));
            Assert.IsTrue(result.Data.Length > 0);

            // First fixture: Barnsley A vs Rotherham A
            Assert.AreEqual("Barnsley A", result.Data[0].Home.Name);
            Assert.AreEqual("Rotherham A", result.Data[0].Away.Name);
            Assert.AreEqual(8, result.Data[0].Result.Home.PointsX2);  // 4 × 2
            Assert.AreEqual(4, result.Data[0].Result.Away.PointsX2);  // 2 × 2
        }
    }
}
