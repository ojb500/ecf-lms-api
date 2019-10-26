using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    public class Competition
    {
        private readonly Organisation _api;
        private readonly string _name;

        internal Competition(Organisation api, string name)
        {
            _api = api;
            _name = name;
        }

        public IEnumerable<Event> GetEvents() => _api.GetEvents(_name);
        public LeagueTable GetTable() => _api.GetTable(_name);
    }
}
