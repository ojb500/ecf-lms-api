using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Ojb500.EcfLms
{
    public class ClubInfo
    {
        private Competition[] _comps;

        internal ClubInfo(Organisation org, string teamPrefix, params string[] competitions)
            :this(teamPrefix, competitions
                .Select(s => org.GetCompetition(s))
                .ToArray())
        {
            
        }

        public ClubInfo(string teamPrefix, params Competition[] competitions)
        {
            _comps = competitions;

            Predicate<string> pred = s => s.StartsWith(teamPrefix);

            Events = _comps
                .SelectMany(c => c.GetEvents())
                .Where(e => pred(e.Home.Name)
                || pred(e.Away.Name))
                .OrderBy(e => e.DateTime)
                .ToArray();
        }

        public Event[] Events { get; }

        public IEnumerable<IEvent> GetRecent() => GetRecent(DateTime.Now);
        public IEnumerable<IEvent> GetRecent(DateTime dt)
        {
            return Events.TakeWhile(e => e.DateTime <= dt).Reverse();
        }

        public IEnumerable<IMatchResult> GetResults() => GetResults(DateTime.Now);
        public IEnumerable<IMatchResult> GetResults(DateTime dt)
        {
            foreach (var evt in GetRecent().Where(e => !e.Result.IsEmpty))
            {
                var comp = evt.Competition;
                var mc = comp.GetMatchCard(evt);
                yield return new MatchResult(evt, mc);
            }
        }


        public IEnumerable<IEvent> GetUpcoming() => GetUpcoming(DateTime.Now);
        public IEnumerable<IEvent> GetUpcoming(DateTime dt)
        {
            return Events.SkipWhile(e => e.DateTime <= dt);
        }

    }
}
