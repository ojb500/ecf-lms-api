using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LmsApi
{

    public class Api
    {
        private HttpClient _hc = CreateHttpClient();
        private JsonSerializer _js = JsonSerializer.CreateDefault();
        private readonly string _org;

        public Competition GetCompetition(string name)
        {
            return new Competition(this, name);
        }
        public ClubInfo GetClub(string name, params string[] competitions)
        {
            return new ClubInfo(this, name, competitions);
        }
        public Api(int orgId)
        {
            _org = orgId.ToString();
        }
        private static HttpClient CreateHttpClient()
        {
            var hc = new HttpClient()
            {
                BaseAddress = new Uri("http://ecflms.org.uk/lms/lmsrest/league/")
            };

            hc.DefaultRequestHeaders.Accept.Clear();
            hc.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return hc;
        }

        internal async Task<Stream> GetJson(string file, string name)
        {
            var result = await _hc.PostAsync(file, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("org", _org),
                new KeyValuePair<string, string>("name", name.ToString())
            })).ConfigureAwait(false);
            //result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync();
        }

        internal T Deserialise<T>(Stream s)
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

        internal string ReadString(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        public IEnumerable<string> GetFixtures(string club)
        {
            var s = ReadString(GetJson("club", club).Result);
            yield return s;
        }
        public LeagueTable GetTable(string competition)
        {
            var s = Deserialise<LeagueTable[]>(GetJson("table", competition).Result);
            return s[0];
        }

        public IEnumerable<string> GetMatches(string competition)
        {
            var s = Deserialise<TableResult<JArray>[]>(GetJson("match", competition).Result);
            yield return "";
        }



        private static Team GetTeam(string s) => new Team(s);
        
        private static DateTime ParseDT(string dts, string t)
        {
            dts = Regex.Replace(dts, "(st|nd|rd|th)", "");
            var dt = DateTime.Parse($"{dts} {t}");
            return dt;
        }

        public IEnumerable<Event> GetEvents(string competition)
        {
            var s = Deserialise<TableResult<JArray>[]>(GetJson("event", competition).Result);
            foreach (var ev in s[0].Data)
            {
                var leftClub = GetTeam(ev[0].Value<string>());
                var (matchLink, result) = HtmlDeparse.DeparseLink(ev[1]);
                var rightClub = GetTeam(ev[2].Value<string>());
                var dts = ev[3].Value<string>();
                var (_, t) = HtmlDeparse.DeparseLink(ev[4]);
                var dt = ParseDT(dts, t);
                yield return new Event(leftClub, result, rightClub, matchLink, dt, competition);
            }
        }

    }
}
