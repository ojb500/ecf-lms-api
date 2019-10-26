using System;
using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    internal class MatchResult : IMatchResult
    {
        private IEvent evt;
        private MatchCard mc;

        public MatchResult(IEvent evt, MatchCard mc)
        {
            this.evt = evt;
            this.mc = mc;
        }

        public Pairing[] Pairings => mc.Pairings;

        public Team Away => evt.Away;

        public string Competition => evt.Competition;

        public DateTime DateTime => evt.DateTime;

        public Team Home => evt.Home;

        public string MatchLink => evt.MatchLink;

        public Score Result => evt.Result;

        public override string ToString()
        {
            return evt.ToString();
        }
    }
}