using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>A season (e.g. "2025-26") with its competition/event ID mappings.</summary>
    public class SeasonWithEvents
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("events")]
        public Dictionary<string, string> Events { get; set; }
    }
}
