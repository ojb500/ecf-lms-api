using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// Represents an ECF LMS organisation (e.g. a county or district chess association).
    /// Obtained via <see cref="ApiExtensions.GetOrganisation"/>.
    /// </summary>
    public class Organisation
    {
        private readonly IModel _api;
        private readonly string _org;
        private readonly Dictionary<string, Competition> _competitions = new Dictionary<string, Competition>();

        internal IModel Model => _api;
        internal string Name => _org;

        /// <summary>
        /// Gets or creates a <see cref="Competition"/> by its ECF LMS name.
        /// Competition instances are cached — the same object is returned for repeated calls.
        /// </summary>
        public Competition GetCompetition(string name)
        {
            if (! _competitions.TryGetValue(name, out var comp))
            {
                comp = _competitions[name] = new Competition(this, name);
            }
            return comp;
        }

        public Organisation(IModel api, int orgId)
        {
            _api = api;
            _org = orgId.ToString();
        }

        /// <summary>Returns the available seasons as a dictionary of ID to season name.</summary>
        public Task<Dictionary<string, string>> GetSeasonsAsync(CancellationToken ct = default)
        {
            return _api.GetSeasonsAsync(_org, ct);
        }

        /// <summary>Returns seasons with their competition/event mappings.</summary>
        public Task<Dictionary<string, SeasonWithEvents>> GetSeasonsWithEventsAsync(CancellationToken ct = default)
        {
            return _api.GetSeasonsWithEventsAsync(_org, ct);
        }

        /// <summary>Returns all fixtures for a club, grouped by competition.</summary>
        public Task<CompetitionEvents[]> GetClubEventsAsync(string clubCode, CancellationToken ct = default)
        {
            return _api.GetClubEventsAsync(_org, clubCode, ct);
        }

        internal Task<MatchCard[]> GetMatchesInternalAsync(string internalName, CancellationToken ct = default)
        {
            return _api.GetMatchCardsAsync(_org, internalName, ct);
        }

        internal Task<Event[]> GetEventsInternalAsync(string internalName, CancellationToken ct = default)
        {
            return _api.GetEventsAsync(_org, internalName, ct);
        }

        internal Task<LeagueTable> GetTableInternalAsync(string internalName, CancellationToken ct = default)
        {
            return _api.GetTableAsync(_org, internalName, ct);
        }
    }
}
