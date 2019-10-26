using Newtonsoft.Json;
using System;

namespace Ojb500.EcfLms.Json
{
    public class PointsApiConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Points);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = reader.ReadAsString();
            s = HtmlDeparse.StripTag(s);
            bool result = Points.TryParse(s.AsSpan(), out var pts, out _);

            if (!result)
            {
                return null;
            }
            return pts;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
