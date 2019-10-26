using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms
{
    public class Organisation
    {
        private readonly IApi _api;
        private readonly string _org;

        public Competition GetCompetition(string name)
        {
            return new Competition(this, name);
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

        public LeagueTable GetTable(string competition)
        {
            return _api.GetOne<LeagueTable>("table", _org, competition);
            
        }
        
        public IEnumerable<Event> GetEvents(string competition)
        {
            var s = _api.GetOne<ApiResult<Event>>("event", _org, competition);
            foreach (var e in s.Data)
            {
                e.Competition = competition;
            }
            return s.Data;
        }
    }
}
