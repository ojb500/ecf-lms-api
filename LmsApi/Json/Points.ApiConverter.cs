using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms.Json
{
    public class PointsApiConverter : JsonConverter<Points>
    {
		public override Points Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var s = reader.GetString();
			s = HtmlDeparse.StripTag(s);
			bool result = Points.TryParse(s.AsSpan(), out var pts, out _);

			if (!result)
			{
				return default;
			}
			return pts;
		}

		public override void Write(Utf8JsonWriter writer, Points value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
    }
}
