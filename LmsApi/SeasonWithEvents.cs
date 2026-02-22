using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    public class SeasonWithEvents
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("events")]
        public Dictionary<string, string> Events { get; set; }
    }
}
