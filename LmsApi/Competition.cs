using System;
using System.Collections.Generic;
using System.Linq;

namespace Ojb500.EcfLms
{
    public class Competition
    {
        private readonly Organisation _api;
        private readonly string _name;
        private string _friendlyName;

        public void SetFriendlyName(string name) => _friendlyName = name;
        public Competition WithFriendlyName(string name)
        {
            SetFriendlyName(name);
            return this;
        }
        internal Competition(Organisation api, string name)
        {
            _api = api;
            _name = name;
        }

        private List<Event> _events;
        private List<MatchCard> _matches;

        public IEnumerable<Event> GetEvents() {
            if (_events == null)
            {
                _events = _api.GetEventsInternal(_name).ToList();
                foreach (var ev in _events)
                {
                    ev.Competition = this;
                }
            }
            return _events;
        }

        
        public IEnumerable<MatchCard> GetMatches() => _matches ?? (_matches = _api.GetMatchesInternal(_name).ToList());
        public LeagueTable GetTable() => _api.GetTableInternal(_name);

        public override string ToString()
        {
            return _friendlyName ?? _name;
        }

        internal MatchCard GetMatchCard(IEvent evt)
        {
            var matches = GetMatches();
            return matches.FirstOrDefault(mc => mc.Date.Date == evt.DateTime.Date && mc.Left.Name == evt.Home.Name && mc.Right.Name == evt.Away.Name);
        }
    }
}
