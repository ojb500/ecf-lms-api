using System;
using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    public class Organisation
    {
        private readonly IApi _api;
        private readonly string _org;
        private readonly Dictionary<string, Competition> _competitions = new Dictionary<string, Competition>();
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
        public Organisation(IApi api, int orgId)
        {
            _api = api;
            _org = orgId.ToString();
        }

        public LeagueTable GetTable(string competition) => GetCompetition(competition).GetTable();

        internal LeagueTable GetTableInternal(string competition)
        {
            return _api.GetOne<LeagueTable>("table", _org, competition);
        }

        public IEnumerable<MatchCard> GetMatches(string competition) => GetCompetition(competition).GetMatches();
        internal IEnumerable<MatchCard> GetMatchesInternal(string competition)
        {
            var s = _api.Get<ApiResult<Pairing>>("match", _org, competition);
            foreach (var m in s)
            {
                var mc = new MatchCard(Team.Parse(m.Header[2]), Team.Parse(m.Header[5]),
                    DateTime.Parse(m.Header[4]),
                    m.Data);
                yield return mc;
            }
        }

        public IEnumerable<Event> GetEvents(string competition) => GetCompetition(competition).GetEvents();
        internal IEnumerable<Event> GetEventsInternal(string competition)
        {
            var s = _api.GetOne<ApiResult<Event>>("event", _org, competition);
            return s.Data;
        }
    }
}
