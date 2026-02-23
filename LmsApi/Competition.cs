using System.Collections.Generic;
using System.Linq;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// A single league or cup competition within an <see cref="Organisation"/>.
    /// Lazily fetches and caches events, match cards, and league table data on first access.
    /// </summary>
    public class Competition
    {
        private readonly Organisation _org;

        /// <summary>The competition name as it appears in the ECF LMS API.</summary>
        public string InternalName { get; }

        private string _friendlyName;

        public void SetFriendlyName(string name) => _friendlyName = name;
        public Competition WithFriendlyName(string name)
        {
            SetFriendlyName(name);
            return this;
        }
        internal Competition(Organisation api, string name)
        {
            _org = api;
            InternalName = name;
        }

        private List<Event> _events;
        private List<MatchCard> _matches;
        private LeagueTable _table;

        /// <summary>Returns all fixtures in this competition.</summary>
        public IEnumerable<Event> GetEvents() {
            if (_events == null)
            {
                _events = _org.GetEventsInternal(InternalName).ToList();
                foreach (var ev in _events)
                {
                    ev.Competition = this;
                }
            }
            return _events;
        }

        /// <summary>Returns detailed match cards with board-by-board pairings.</summary>
        public IEnumerable<MatchCard> GetMatches() => _matches ?? (_matches = _org.GetMatchesInternal(InternalName).ToList());

        /// <summary>Returns the league table for this competition.</summary>
        public LeagueTable GetTable()
        {
            if (_table != null)
            {
                return _table;
            }

            _table = _org.GetTableInternal(InternalName);
            _table.Competition = this;
            return _table;
        }


        public override string ToString()
        {
            return Name;
        }

        /// <summary>The friendly name if set, otherwise <see cref="InternalName"/>.</summary>
        public string Name => _friendlyName ?? InternalName;
    }
}
