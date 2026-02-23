using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Ojb500.EcfLms.Test
{
    [TestClass]
    public class SeasonsParsingTests
    {
        private static string ReadSample(string filename)
            => File.ReadAllText(Path.Combine("Samples", filename));

        [TestMethod]
        public void ParseSeasonsResponse()
        {
            var json = ReadSample("seasons.json");
            var seasons = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            Assert.IsTrue(seasons.Count > 0);
            Assert.AreEqual("2025-26", seasons["1734"]);
            Assert.AreEqual("2024-25", seasons["626"]);
            Assert.AreEqual("2023-24", seasons["633"]);
        }

        [TestMethod]
        public void ParseSeasonsWithEventsResponse()
        {
            var json = ReadSample("seasonsWithEvents.json");
            var seasons = JsonSerializer.Deserialize<Dictionary<string, SeasonWithEvents>>(json);

            Assert.IsTrue(seasons.Count > 0);

            var current = seasons["1734"];
            Assert.AreEqual("2025-26", current.Name);
            Assert.IsTrue(current.Events.Count > 0);
            Assert.AreEqual("Div 1 - Davy Trophy", current.Events["8627"]);
            Assert.AreEqual("Richardson Cup", current.Events["8632"]);
        }
    }
}
