using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ojb500.EcfLms.Json
{
    public class PairingApiConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Pairing);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var ja = JArray.ReadFrom(reader);

            return new Pairing(ja[1].Value<int>(), ja[0].Value<string>() == "W",
            new Player(ja[2].Value<string>(), ja[3].Value<string>()),
            new Player(ja[5].Value<string>(), ja[6].Value<string>()),
            GameResult.Parse(ja[4].Value<string>()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
