using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms.Json
{
    public class LeagueTableEntryApiConverter : JsonConverter<LeagueTableEntry>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LeagueTableEntry);
        }

		public override LeagueTableEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var ja = (JsonArray) JsonNode.Parse(ref reader);
			var team = Team.Parse(ja[0].AsValue().GetValue<string>());
			var p = ja[1].AsValue().GetValue<int>();
			var w = ja[2].AsValue().GetValue<int>();
			var d = ja[3].AsValue().GetValue<int>();
			var l = ja[4].AsValue().GetValue<int>();
			var f = new Points(ja[5].AsValue().GetValue<string>());
			var a = new Points(ja[6].AsValue().GetValue<string>());
			var pts = new Points(HtmlDeparse.StripTag(ja[7].AsValue().GetValue<string>()));

			return new LeagueTableEntry(team, p, w, d, l, f, a, pts);
			throw new NotImplementedException();
		}

		public override void Write(Utf8JsonWriter writer, LeagueTableEntry value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
    }
}
