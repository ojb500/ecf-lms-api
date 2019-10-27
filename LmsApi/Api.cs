using Ojb500.EcfLms.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace Ojb500.EcfLms
{
    public interface IModel
    {
        LeagueTable GetTable(string org, string name);
        IEnumerable<Event> GetEvents(string org, string name);
        IEnumerable<MatchCard> GetMatchCards(string org, string name);
    }

    public class FileApi : IModel
    {
        private static Regex _toPath = new Regex(@"[^a-zA-Z0-9]");
        private static string _basePath;
        public FileApi(string basePath)
        {
            _basePath = basePath;
        }
        private string GetPath(string file, string org, string name)
        {
            var safeName = _toPath.Replace(name, "");
            return Path.Combine(_basePath, $"{org}/{safeName}/{file}.json");
        }
        private static JsonSerializer _js = CreateSerializer();

        private static JsonSerializer CreateSerializer()
        {
            var js = JsonSerializer.CreateDefault();
            js.Formatting = Formatting.Indented;
            return js;
        }
               
        public void Update(IModel remote, int orgId, bool getTables = true, params string[] competitions)
        {
            Update(remote, orgId, Console.WriteLine, getTables, competitions);
        }
        public void Update(IModel remote, int orgId, Action<string> log, bool getTables = true, params string[] competitions)
        {
            var org = orgId.ToString();

            foreach (var comp in competitions)
            {
                UpdateInternal(remote, org, comp, log, getTables);
            }
        }

        private void UpdateInternal(IModel remote, string org, string comp, Action<string> log, bool getTables)
        {
            var localEvents = GetEvents(org, comp);
            var events = remote.GetEvents(org, comp).ToArray();

            if (localEvents != null && localEvents.Data.Length > events.Length)
            {
                throw new InvalidDataException("Events went missing");
            }
            PutEvents(org, comp, events);

            var cards = remote.GetMatchCards(org, comp).ToArray();
            var localCards = GetMatchCards(org, comp);
            if (localCards != null && localCards.Data.Length > events.Length)
            {
                throw new InvalidDataException("Cards went missing");
            }
            PutMatchCards(org, comp, cards);

            if (getTables)
            {
                LeagueTable lt = remote.GetTable(org, comp);
                var localTable = GetTable(org, comp);

                if (lt.Data.Length == 0)
                {
                    throw new InvalidDataException("Empty league table");
                }

                PutTable(org, comp, lt);
            }

        }

        private class SavedThing<T>
        {
            public DateTime Saved { get; set; }
            public T Data { get; set; }
        }

        private void Write<T>(string file, string org, string name, T thing)
        {
            var path = GetPath(file, org, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var sw = new StringWriter())
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    _js.Serialize(jw, new SavedThing<T> { Saved = DateTime.Now, Data = thing });

                }
                var s = sw.ToString();
                File.WriteAllText(path, s);
            }
        }


        private SavedThing<T> Read<T>(string file, string org, string name)
        {
            var path = GetPath(file, org, name);
            if (File.Exists(path))
            {
                try
                {
                    using (var sw = File.OpenRead(path))
                    {
                        using (var sr = new StreamReader(sw))
                        {
                            using (var jw = new JsonTextReader(sr))
                            {
                                return _js.Deserialize<SavedThing<T>>(jw);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return null;
        }

        private SavedThing<LeagueTable> GetTable(string org, string name)
            => Read<LeagueTable>("table", org, name);
        private void PutTable(string org, string name, LeagueTable table)
           => Write("table", org, name, table);

        private SavedThing<Event[]> GetEvents(string org, string name)
            => Read<Event[]>("event", org, name);
        private void PutEvents(string org, string name, Event[] events)
            => Write("event", org, name, events);

        private SavedThing<MatchCard[]> GetMatchCards(string org, string name)
            => Read<MatchCard[]>("match", org, name);
        private void PutMatchCards(string org, string name, MatchCard[] cards)
            => Write("match", org, name, cards);

        LeagueTable IModel.GetTable(string org, string name)
        {
            var t = GetTable(org, name);
            return t.Data;
        }

        IEnumerable<Event> IModel.GetEvents(string org, string name)
        {
            var ev = GetEvents(org, name);
            return ev.Data;
        }

        IEnumerable<MatchCard> IModel.GetMatchCards(string org, string name)
        {
            var ev = GetMatchCards(org, name);
            return ev.Data;
        }
    }
    public class Api : IModel
    {
        public static Api Default = new Api();


        public Api() : this("http://ecflms.org.uk/lms/lmsrest/league/")
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
            Trace.WriteLine($"Requesting {file} with org={org}, name='{name}'");
            var result = await _hc.PostAsync(file, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                new KeyValuePair<string, string>("org", org),
                new KeyValuePair<string, string>("name", name)
                })).ConfigureAwait(false);

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
