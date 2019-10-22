using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LmsApi
{
    [JsonConverter(typeof(LeagueTableEntry.Converter))]
    public class LeagueTableEntry
    {
        public LeagueTableEntry(JToken ja)
        {
            Team = new Team(ja[0].Value<string>());
            P = ja[1].Value<int>();
            W = ja[2].Value<int>();
            D = ja[3].Value<int>();
            L = ja[4].Value<int>();
            F = new Points(ja[5].Value<string>());
            A = new Points(ja[6].Value<string>());
            Pts = new Points(HtmlDeparse.StripTag(ja[7].Value<string>()));
        }
        
        public Team Team { get; }
        public int P { get; }
        public int W { get; }
        public int D { get; }
        public int L { get; }
        public Points F { get; }
        public Points A { get; }
        public Points Pts { get; }

        internal class Converter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(LeagueTableEntry);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var ja = JArray.ReadFrom(reader);
                var lte = new LeagueTableEntry(ja);

                return lte;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
        public override string ToString()
        {
            return $"{Team.Abbreviated} (P{P} W{W} D{D} L{L} BD{F - A})";
        }
    }
}
