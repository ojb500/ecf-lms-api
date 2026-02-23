using Ojb500.EcfLms.Json;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>A single player row in a Swiss-system crosstable.</summary>
    [JsonConverter(typeof(CrosstableEntryApiConverter))]
    public class CrosstableEntry
    {
        public CrosstableEntry(int seedNumber, string name, RoundResult[] results, Points total)
        {
            Rank = seedNumber;
            Name = name;
            Results = results;
            Total = total;
        }

        /// <summary>The player's seed/draw number.</summary>
        public int Rank { get; }
        /// <summary>The player's name.</summary>
        public string Name { get; }
        /// <summary>Results for each round.</summary>
        public RoundResult[] Results { get; }
        /// <summary>Total score.</summary>
        public Points Total { get; }

        public override string ToString()
        {
            return $"{Rank}. {Name} ({Total})";
        }
    }
}
