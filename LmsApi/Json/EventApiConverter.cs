using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms.Json
{
    internal class EventApiConverter : JsonConverter<Event>
    {
        private static readonly string[] DateFormats = new[]
        {
            "ddd d MMM yy",   // "Mon 29 Sep 25"
            "ddd dd MMM yy",  // "Wed 01 Oct 25"
        };

        private static DateTime? ParseDT(string dts, string time)
        {
            if (string.IsNullOrEmpty(dts) || dts == "Postponed" || dts.StartsWith("Not Set"))
            {
                return default;
            }

            if (DateTime.TryParseExact(dts.Trim(), DateFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            {
                if (TimeSpan.TryParse(time, out var ts))
                {
                    date = date.Add(ts);
                }
                return date;
            }

            // Fallback: try general parse
            if (DateTime.TryParse($"{dts} {time}", out var dt))
            {
                return dt;
            }

            return default;
        }

		public override Event Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var ev = (JsonArray) JsonNode.Parse(ref reader);

			var leftClub = Team.Parse(ev[0].AsValue().GetValue<string>());
			var result = ev[1].AsValue().GetValue<string>();
			var rightClub = Team.Parse(ev[2].AsValue().GetValue<string>());
			var dts = ev[3].AsValue().GetValue<string>();
			var time = ev[4].AsValue().GetValue<string>();
			var dt = ParseDT(dts, time);
			return new Event(leftClub, result, rightClub, null, dt, null);
		}

		public override void Write(Utf8JsonWriter writer, Event value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}
