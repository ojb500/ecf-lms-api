﻿using System;
using System.Collections.Generic;
using System.Linq;

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


        public bool IsDefault => Pairings.All(p => p.FirstPlayer.IsDefault || p.SecondPlayer.IsDefault);

        public Pairing[] Pairings => mc?.Pairings ?? Array.Empty<Pairing>();

        public Team Away => evt.Away;

        public Competition Competition => evt.Competition;
        public DateTime? DateTime => evt.DateTime;

        public Team Home => evt.Home;

        public string MatchLink => evt.MatchLink;

        public Score Result => evt.Result;

        public override string ToString()
        {
            return evt.ToString();
        }
    }
}