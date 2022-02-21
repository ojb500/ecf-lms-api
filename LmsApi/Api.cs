using Ojb500.EcfLms.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Ojb500.EcfLms
{
    public class Api : IModel
    {
        public static Api Default = new Api();


        public Api() : this("https://ecflms.org.uk/lms/lmsrest/league/")
        {
        }
        public Api(string baseAddress)
        {
            _baseAddress = baseAddress;
            _hc = CreateHttpClient(baseAddress);
            _js = JsonSerializer.CreateDefault();
            _js.Converters.Add(new LeagueTableEntryApiConverter());
            _js.Converters.Add(new PointsApiConverter());
            _js.Converters.Add(new EventApiConverter());
            _js.Converters.Add(new PairingApiConverter());
        }


        private async Task<Stream> GetJson(string file, string org, string name)
        {
            file = file + ".json";
            Trace.WriteLine($"Requesting {file} with org={org}, name='{name}'");
            var result = await _hc.PostAsync(file, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                new KeyValuePair<string, string>("org", org),
                new KeyValuePair<string, string>("name", name)
                })).ConfigureAwait(false);
            
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private readonly string _baseAddress;
        private readonly HttpClient _hc;
        private readonly JsonSerializer _js;

        private HttpClient CreateHttpClient(string baseAddress)
        {
            var hc = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };

            hc.DefaultRequestHeaders.Accept.Clear();
            hc.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return hc;
        }
        private T Deserialise<T>(Stream s)
        {
            var str = ReadString(s);

            using (var sr = new StringReader(str))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    return _js.Deserialize<T>(jtr);
                }
            }
        }

        private string ReadString(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        private T[] Get<T>(string file, string org, string name)
        {
            return Deserialise<T[]>(GetJson(file, org, name).Result);
        }

        private T GetOne<T>(string file, string org, string name)
        {
            var results = Get<T>(file, org, name);
            if (results.Length != 1)
            {
                if (results.Length > 1)
                {
                    throw new InvalidOperationException($"Expected 1 result, got {results.Length}");
                }
                return default;
            }
            return results[0];
        }

        LeagueTable IModel.GetTable(string org, string name)
        {
            return GetOne<LeagueTable>("table", org, name);
        }

        IEnumerable<Event> IModel.GetEvents(string org, string name)
        {
            var s = GetOne<ApiResult<Event>>("event", org, name);
            return s.Data;
        }

        IEnumerable<MatchCard> IModel.GetMatchCards(string org, string name)
        {
            var s = Get<ApiResult<Pairing>>("match", org, name);
            foreach (var m in s)
            {
                var mc = new MatchCard(Team.Parse(m.Header[2]), Team.Parse(m.Header[5]),
                    DateTime.Parse(m.Header[4]),
                    m.Data);
                yield return mc;
            }
        }
    }
}
