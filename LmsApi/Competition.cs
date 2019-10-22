using System.Collections.Generic;

namespace LmsApi
{
    public class Competition
    {
        private readonly Api _api;
        private readonly string _name;

        internal Competition(Api api, string name)
        {
            _api = api;
            _name = name;
        }

        public IEnumerable<Event> GetEvents() => _api.GetEvents(_name);
        public LeagueTable GetTable() => _api.GetTable(_name);
    }
}
