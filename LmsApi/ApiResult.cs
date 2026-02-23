using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    internal class ApiResult<T>
    {
        [JsonPropertyName("data")]
        public T[] Data { get; set; }

        [JsonPropertyName("header")]
        public string[] Header { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Data.Length} rows)";
        }
    }
}
