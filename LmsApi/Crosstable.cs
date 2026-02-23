using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// A Swiss-system individual tournament crosstable with round-by-round results.
    /// </summary>
    public class Crosstable
    {
        public string Title { get; internal set; }
        public string[] Rounds { get; internal set; }
        public CrosstableEntry[] Entries { get; internal set; }

        /// <summary>The friendly competition name if available, otherwise <see cref="Title"/>.</summary>
        public string Name => Competition?.Name ?? Title;

        [JsonIgnore]
        internal Competition Competition { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Entries.Length} entries)";
        }
    }
}
