using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        private Event[] _events;
        private MatchCard[] _matches;
        private LeagueTable _table;

        /// <summary>Returns all fixtures in this competition.</summary>
        public async Task<Event[]> GetEventsAsync(CancellationToken ct = default) {
            if (_events == null)
            {
                _events = await _org.GetEventsInternalAsync(InternalName, ct).ConfigureAwait(false);
                foreach (var ev in _events)
                {
                    ev.Competition = this;
                }
            }
            return _events;
        }

        /// <summary>Returns detailed match cards with board-by-board pairings.</summary>
        public async Task<MatchCard[]> GetMatchesAsync(CancellationToken ct = default)
        {
            if (_matches == null)
            {
                _matches = await _org.GetMatchesInternalAsync(InternalName, ct).ConfigureAwait(false);
            }
            return _matches;
        }

        /// <summary>Returns the league table for this competition.</summary>
        public async Task<LeagueTable> GetTableAsync(CancellationToken ct = default)
        {
            if (_table != null)
            {
                return _table;
            }

            _table = await _org.GetTableInternalAsync(InternalName, ct).ConfigureAwait(false);
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
