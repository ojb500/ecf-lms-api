using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ojb500.EcfLms.Json
{
    public partial class LeagueTableEntryApiConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LeagueTableEntry);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var ja = JArray.ReadFrom(reader);
            var team = Team.Parse(ja[0].Value<string>());
            var p = ja[1].Value<int>();
            var w = ja[2].Value<int>();
            var d = ja[3].Value<int>();
            var l = ja[4].Value<int>();
            var f = new Points(ja[5].Value<string>());
            var a = new Points(ja[6].Value<string>());
            var pts = new Points(HtmlDeparse.StripTag(ja[7].Value<string>()));

            return new LeagueTableEntry(team, p, w, d, l, f, a, pts);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
