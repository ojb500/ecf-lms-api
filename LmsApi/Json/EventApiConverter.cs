using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms.Json
{
    public class EventApiConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Event);
        }

        private static DateTime ParseDT(string dts, string t)
        {
            dts = Regex.Replace(dts, "(st|nd|rd|th)", "");
            var dt = DateTime.Parse($"{dts} {t}");
            return dt;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var ev = JArray.ReadFrom(reader);

            var leftClub = Team.Parse(ev[0].Value<string>());
            var (matchLink, result) = HtmlDeparse.DeparseLink(ev[1]);
            var rightClub = Team.Parse(ev[2].Value<string>());
            var dts = ev[3].Value<string>();
            var (_, t) = HtmlDeparse.DeparseLink(ev[4]);
            var dt = ParseDT(dts, t);
            return new Event(leftClub, result, rightClub, matchLink, dt, null);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
