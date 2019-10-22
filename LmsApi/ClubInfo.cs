﻿using System;
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

        public (List<Event> recent, List<Event> upcoming) GetRecent(int n = 5) => GetRecent(DateTime.Now, n);
        public (List<Event> recent, List<Event> upcoming) GetRecent(DateTime dt, int n = 5)
        {
            var fixt = Fixtures;
            int future;
            for (future = 0; future < fixt.Length; future++)
            {
                if (fixt[future].DateTime > dt)
                    break;
            }

            List<Event> upcoming = new List<Event>();
            for (int i = future; i < fixt.Length && i < future + 5; i++)
            {
                upcoming.Add(fixt[i]);
            }

            List<Event> recents = new List<Event>();
            var endOfRecents = future;
            var firstRecent = Math.Max(0, endOfRecents - 5);
            for (int i = firstRecent; i < endOfRecents; i++)
            {
                recents.Add(fixt[i]);
            }

            return (recents, upcoming);
        }


    }
}
