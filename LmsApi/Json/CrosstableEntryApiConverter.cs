using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms.Json
{
    internal class CrosstableEntryApiConverter : JsonConverter<CrosstableEntry>
    {
        private static string NodeAsString(JsonNode node)
        {
            var val = node.AsValue();
            if (val.TryGetValue<string>(out var s)) return s;
            if (val.TryGetValue<int>(out var i)) return i.ToString();
            return val.ToJsonString();
        }

        public override CrosstableEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var ja = (JsonArray)JsonNode.Parse(ref reader);

            // Row format: [seed, name, round1, round2, ..., total]
            var seed = ja[0].AsValue().GetValue<int>();
            var name = ja[1].AsValue().GetValue<string>();

            int roundCount = ja.Count - 3;
            var results = new RoundResult[roundCount];
            for (int i = 0; i < roundCount; i++)
            {
                results[i] = RoundResult.Parse(NodeAsString(ja[i + 2]));
            }

            var total = new Points(NodeAsString(ja[ja.Count - 1]));

            return new CrosstableEntry(seed, name, results, total);
        }

        public override void Write(Utf8JsonWriter writer, CrosstableEntry value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
