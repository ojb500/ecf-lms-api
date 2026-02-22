using System;
using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    public class Organisation
    {
        private readonly IModel _api;
        private readonly string _org;
        private readonly Dictionary<string, Competition> _competitions = new Dictionary<string, Competition>();

        internal IModel Model => _api;
        internal string Name => _org;


        public Competition GetCompetition(string name)
        {
            if (! _competitions.TryGetValue(name, out var comp))
            {
                comp = _competitions[name] = new Competition(this, name);
            }
            return comp;
        }
        public ClubInfo GetClub(string name, params string[] competitions)
        {
            return new ClubInfo(this, name, competitions);
        }
        public ClubInfo GetClub(string name, params Competition[] competitions)
        {
            return new ClubInfo(name, competitions);
        }

        public Organisation(IModel api, int orgId)
        {
            _api = api;
            _org = orgId.ToString();
        }

        public Dictionary<string, string> GetSeasons()
        {
            return _api.GetSeasons(_org);
        }

        public Dictionary<string, SeasonWithEvents> GetSeasonsWithEvents()
        {
            return _api.GetSeasonsWithEvents(_org);
        }

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
