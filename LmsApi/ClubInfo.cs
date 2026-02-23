using System;
using System.Collections.Generic;
using System.Linq;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// Aggregates fixtures and results for a single club across one or more competitions.
    /// </summary>
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
                .OrderBy(e => e.DateTime.GetValueOrDefault(new DateTime(3000, 1, 1)))
                .ToArray();
        }

        /// <summary>All fixtures involving this club, ordered by date.</summary>
        public Event[] Events { get; }

        /// <summary>Played fixtures in reverse chronological order.</summary>
        public IEnumerable<IEvent> GetRecent() => GetRecent(DateTime.Now);

        /// <inheritdoc cref="GetRecent()"/>
        public IEnumerable<IEvent> GetRecent(DateTime dt)
        {
            return Events.TakeWhile(e => e.DateTime <= dt).Reverse();
        }

        /// <summary>
        /// Played fixtures paired with their detailed match cards, in reverse chronological order.
        /// </summary>
        public IEnumerable<(Event Event, MatchCard MatchCard)> GetResults() => GetResults(DateTime.Now);

        /// <inheritdoc cref="GetResults()"/>
        public IEnumerable<(Event Event, MatchCard MatchCard)> GetResults(DateTime dt)
        {
            foreach (var evt in GetRecent(dt).Where(e => !e.Result.IsEmpty))
            {
                var comp = evt.Competition;
                var mc = comp.GetMatchCard(evt);
                yield return ((Event)evt, mc);
            }
        }

        /// <summary>Upcoming fixtures in chronological order.</summary>
        public IEnumerable<IEvent> GetUpcoming() => GetUpcoming(DateTime.Now);

        /// <inheritdoc cref="GetUpcoming()"/>
        public IEnumerable<IEvent> GetUpcoming(DateTime dt)
        {
            return Events.SkipWhile(e => e.DateTime <= dt);
        }
    }
}
