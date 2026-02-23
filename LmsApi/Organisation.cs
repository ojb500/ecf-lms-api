using System.Collections.Generic;

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

        /// <summary>
        /// Creates a <see cref="ClubInfo"/> that aggregates fixtures for a club
        /// across the named competitions.
        /// </summary>
        public ClubInfo GetClub(string name, params string[] competitions)
        {
            return new ClubInfo(this, name, competitions);
        }

        /// <inheritdoc cref="GetClub(string, string[])"/>
        public ClubInfo GetClub(string name, params Competition[] competitions)
        {
            return new ClubInfo(name, competitions);
        }

        public Organisation(IModel api, int orgId)
        {
            _api = api;
            _org = orgId.ToString();
        }

        /// <summary>Returns the available seasons as a dictionary of ID to season name.</summary>
        public Dictionary<string, string> GetSeasons()
        {
            return _api.GetSeasons(_org);
        }

        /// <summary>Returns seasons with their competition/event mappings.</summary>
        public Dictionary<string, SeasonWithEvents> GetSeasonsWithEvents()
        {
            return _api.GetSeasonsWithEvents(_org);
        }

        /// <summary>Returns all fixtures for a club, grouped by competition.</summary>
        public IEnumerable<ApiResult<Event>> GetClubEvents(string clubCode)
        {
            return _api.GetClubEvents(_org, clubCode);
        }

        internal IEnumerable<MatchCard> GetMatchesInternal(string internalName)
        {
            return _api.GetMatchCards(_org, internalName);
        }

        internal IEnumerable<Event> GetEventsInternal(string internalName)
        {
            return _api.GetEvents(_org, internalName);
        }

        internal LeagueTable GetTableInternal(string internalName)
        {
            return _api.GetTable(_org, internalName);
        }
    }
}
