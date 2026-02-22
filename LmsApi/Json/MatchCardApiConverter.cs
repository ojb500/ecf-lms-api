using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms.Json
{
    public class PairingApiConverter : JsonConverter<Pairing>
    {
        private static readonly Regex BoardRegex = new Regex(@"(\d+)\s*\(\s*([WB])\s*\)");

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Pairing);
        }

		public override Pairing Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			// Data array can contain adjustment rows (arrays) mixed with pairings (objects).
			// Return null for non-object elements; caller filters nulls.
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				// Skip the entire array
				JsonNode.Parse(ref reader);
				return null;
			}

			var obj = (JsonObject) JsonNode.Parse(ref reader);

			var boardStr = obj["board"]?.GetValue<string>() ?? "";
			var boardMatch = BoardRegex.Match(boardStr);
			int board = boardMatch.Success ? int.Parse(boardMatch.Groups[1].Value) : 0;
			bool isWhite = boardMatch.Success && boardMatch.Groups[2].Value == "W";

			var hname = obj["hname"]?.GetValue<string>() ?? "";
			var hrating = obj["hrating"]?.GetValue<string>() ?? "";
			var aname = obj["aname"]?.GetValue<string>() ?? "";
			var arating = obj["arating"]?.GetValue<string>() ?? "";
			var result = obj["result"]?.GetValue<string>() ?? "";

			return new Pairing(board, isWhite,
				new Player(hname, new Grade(hrating)),
				new Player(aname, new Grade(arating)),
				GameResult.Parse(result));
		}

		public override void Write(Utf8JsonWriter writer, Pairing value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
    }
}
