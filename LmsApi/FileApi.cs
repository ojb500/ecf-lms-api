//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Linq;

//namespace Ojb500.EcfLms
//{
//    public class FileApi : IModel
//    {
//        private static Regex _toPath = new Regex(@"[^a-zA-Z0-9]");
//        private static string _basePath;
//        public FileApi(string basePath)
//        {
//            _basePath = basePath;
//        }
//        private string GetPath(string file, string org, string name)
//        {
//            var safeName = _toPath.Replace(name, "");
//            return Path.Combine(_basePath, $"{org}/{safeName}/{file}.json");
//        }

//        public void Update(IModel remote, int orgId, bool getTables = true, params string[] competitions)
//        {
//            Update(remote, orgId, Console.WriteLine, getTables, competitions);
//        }
//        public void Update(IModel remote, int orgId, Action<string> log, bool getTables = true, params string[] competitions)
//        {
//            var org = orgId.ToString();

//            foreach (var comp in competitions)
//            {
//                UpdateInternal(remote, org, comp, log, getTables);
//            }
//        }

//        private void UpdateInternal(IModel remote, string org, string comp, Action<string> log, bool getTables)
//        {
//            var localCards = GetMatchCards(org, comp);
//            var cards = remote.GetMatchCards(org, comp).ToArray();
//            if (localCards != null && localCards.Data.Length > cards.Length)
//            {
//                throw new InvalidDataException("Cards went missing");
//            }
//            PutMatchCards(org, comp, cards);

//            var localEvents = GetEvents(org, comp);
//            var events = remote.GetEvents(org, comp).ToArray();

//            if (localEvents != null && localEvents.Data.Length > events.Length)
//            {
//                throw new InvalidDataException("Events went missing");
//            }
//            PutEvents(org, comp, events);

//            if (getTables)
//            {
//                LeagueTable lt = remote.GetTable(org, comp);
//                var localTable = GetTable(org, comp);

//                if (lt.Data.Length == 0)
//                {
//                    throw new InvalidDataException("Empty league table");
//                }

//                PutTable(org, comp, lt);
//            }

//        }

//        private class SavedThing<T>
//        {
//            public DateTime Saved { get; set; }
//            public T Data { get; set; }
//        }

//        private void Write<T>(string file, string org, string name, T thing)
//        {
//            var path = GetPath(file, org, name);
//            Directory.CreateDirectory(Path.GetDirectoryName(path));
//            using (var sw = new StringWriter())
//            {
//                using (var jw = new JsonTextWriter(sw))
//                {
//                    _js.Serialize(jw, new SavedThing<T> { Saved = DateTime.Now, Data = thing });

//                }
//                var s = sw.ToString();
//                File.WriteAllText(path, s);
//            }
//        }


//        private SavedThing<T> Read<T>(string file, string org, string name)
//        {
//            var path = GetPath(file, org, name);
//            if (File.Exists(path))
//            {
//                try
//                {
//                    using (var sw = File.OpenRead(path))
//                    {
//                        using (var sr = new StreamReader(sw))
//                        {
//                            using (var jw = new JsonTextReader(sr))
//                            {
//                                return _js.Deserialize<SavedThing<T>>(jw);
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex);
//                }
//            }
//            return null;
//        }

//        private SavedThing<LeagueTable> GetTable(string org, string name)
//            => Read<LeagueTable>("table", org, name);
//        private void PutTable(string org, string name, LeagueTable table)
//           => Write("table", org, name, table);

//        private SavedThing<Event[]> GetEvents(string org, string name)
//            => Read<Event[]>("event", org, name);
//        private void PutEvents(string org, string name, Event[] events)
//            => Write("event", org, name, events);

//        private SavedThing<MatchCard[]> GetMatchCards(string org, string name)
//            => Read<MatchCard[]>("match", org, name);
//        private void PutMatchCards(string org, string name, MatchCard[] cards)
//            => Write("match", org, name, cards);

//        LeagueTable IModel.GetTable(string org, string name)
//        {
//            var t = GetTable(org, name);
//            return t.Data;
//        }

//        IEnumerable<Event> IModel.GetEvents(string org, string name)
//        {
//            var ev = GetEvents(org, name);
//            return ev.Data;
//        }

//        IEnumerable<MatchCard> IModel.GetMatchCards(string org, string name)
//        {
//            var ev = GetMatchCards(org, name);
//            return ev.Data;
//        }
//    }
//}
