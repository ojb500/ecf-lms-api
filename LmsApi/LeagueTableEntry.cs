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
            F = ja[5].Value<string>();
            A = ja[6].Value<string>();
            Pts = ja[7].Value<string>();
        }

        public Team Team { get; }
        public int P { get; }
        public int W { get; }
        public int D { get; }
        public int L { get; }
        public string F { get; }
        public string A { get; }
        public string Pts { get; }

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
    }
}
