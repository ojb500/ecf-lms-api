using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LmsApi
{
    public class ClubInfo
    {
        private Api _api;
        private string _prefix;
        private string[] _comps;

        internal ClubInfo(Api api, string teamPrefix, params string[] competitions)
        {
            _api = api;
            _prefix = teamPrefix;
            _comps = competitions;
            Predicate<string> pred = s => s.StartsWith(teamPrefix);

            Fixtures = competitions
                .Select(s => _api.GetCompetition(s))
                .SelectMany(c => c.GetEvents())
                .Where(e => pred(e.Home.Name)
                || pred(e.Away.Name))
                .OrderBy(e => e.DateTime)
                .ToArray();
        }

        public Event[] Fixtures { get; }

        public EventCollection GetRecent(int n = 5) => GetRecent(DateTime.Now, n);
        public EventCollection GetRecent(DateTime dt, int n = 5)
        {
            var fixt = Fixtures;
            int future;
            for (future = 0; future < fixt.Length; future++)
            {
                if (fixt[future].DateTime > dt)
                    break;
            }

            List<Event> upcoming = new List<Event>();
            for (int i = future; i < fixt.Length && i < future + n; i++)
            {
                upcoming.Add(fixt[i]);
            }

            List<Event> recents = new List<Event>();
            var endOfRecents = future;
            for (int i = endOfRecents - 1; i >= 0 && recents.Count < n; i--)
            {
                if (fixt[i].Result.IsEmpty)
                {
                    continue;
                }
                recents.Add(fixt[i]);
            }

            return new EventCollection(recents, upcoming);
        }


    }
}
