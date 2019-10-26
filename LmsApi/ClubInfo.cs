using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Ojb500.EcfLms
{
    public class ClubInfo
    {
        private Organisation _org;
        private string _prefix;
        private string[] _comps;

        internal ClubInfo(Organisation org, string teamPrefix, params string[] competitions)
        {
            _org = org;
            _prefix = teamPrefix;
            _comps = competitions;
            Predicate<string> pred = s => s.StartsWith(teamPrefix);

            Events = competitions
                .Select(s => _org.GetCompetition(s))
                .SelectMany(c => c.GetEvents())
                .Where(e => pred(e.Home.Name)
                || pred(e.Away.Name))
                .OrderBy(e => e.DateTime)
                .ToArray();
        }

        public Event[] Events { get; }

        public IEnumerable<Event> GetRecent() => GetRecent(DateTime.Now);
        public IEnumerable<Event> GetRecent(DateTime dt)
        {
            return Events.TakeWhile(e => e.DateTime <= dt).Reverse();
        }

        public IEnumerable<Event> GetResults() => GetResults(DateTime.Now);
        public IEnumerable<Event> GetResults(DateTime dt)
        {
            return GetRecent().Where(e => !e.Result.IsEmpty);
        }


        public IEnumerable<Event> GetUpcoming() => GetUpcoming(DateTime.Now);
        public IEnumerable<Event> GetUpcoming(DateTime dt)
        {
            return Events.SkipWhile(e => e.DateTime <= dt);
        }

    }
}
