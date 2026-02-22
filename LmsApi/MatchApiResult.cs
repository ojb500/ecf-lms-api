using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    internal class MatchApiResult
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("header")]
        public string[] Header { get; set; }

        [JsonPropertyName("data")]
        public JsonElement[] Data { get; set; }
    }
}
