﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ojb500.EcfLms
{
    public class Competition
    {
        private readonly Organisation _org;
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

        
        public IEnumerable<MatchCard> GetMatches() => _matches ?? (_matches = _org.GetMatchesInternal(InternalName).ToList());
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

        public string Name => _friendlyName ?? InternalName;

        internal MatchCard GetMatchCard(IEvent evt)
        {
            var matches = GetMatches();
            return matches.FirstOrDefault(mc => mc.Date.GetValueOrDefault().Date == evt.DateTime.GetValueOrDefault().Date && mc.Left.Name == evt.Home.Name && mc.Right.Name == evt.Away.Name);
        }
    }
}
