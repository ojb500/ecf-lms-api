using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LmsApi
{
    [JsonConverter(typeof(Points.Converter))]
    public struct Points
    {
        private class Converter : JsonConverter
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

        private static bool TryParse(ReadOnlySpan<char> readOnlySpan, out Points points, out int charsConsumed)
        {
            int pts = 0;
            bool half = false;
            bool any = false;
            int i = 0;
            for (i = 0; i < readOnlySpan.Length && char.IsWhiteSpace(readOnlySpan[i]); i++)
            {

            }

            for (; i < readOnlySpan.Length; i++)
            {
                char c = readOnlySpan[i];
                if (c == '½')
                {
                    any = true;
                    half = true;
                    break;
                }
                if (c >= '0' && c <= '9')
                {
                    any = true;
                    int v = c - '0';
                    pts = (pts * 10) + v;
                }
            }


            charsConsumed = i;
            if (!any)
            {
                points = default;
                return false;
            }

            pts = pts * 2;
            if (half)
            {
                pts++;
            }
            points = new Points(pts);
            return true;
        }

        private readonly int _ptsx2;

        public Points(string pts)
        {
            var result = Points.TryParse(pts.AsSpan(), out this, out _);
        }
        public Points(int pts_x2)
        {
            _ptsx2 = pts_x2;
        }

        public override string ToString()
        {
            bool half = (_ptsx2 & 1) != 0;
            int pts = _ptsx2 >> 1;
            if (half)
                return $"{pts}½";
            return pts.ToString();
        }


        public static Points operator -(Points a, Points b) => new Points(a._ptsx2 - b._ptsx2);
        public static Points operator +(Points a, Points b) => new Points(a._ptsx2 + b._ptsx2);
    }
}
