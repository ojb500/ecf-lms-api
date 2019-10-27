using System;

namespace Ojb500.EcfLms
{
    public class MatchCard
    {
        public MatchCard(Team left, Team right, DateTime date, Pairing[] pairings)
        {
            Left = left;
            Right = right;
            Date = date;
            Pairings = pairings;
        }

        public Team Left { get; set; }
        public Team Right { get; set; }
        public DateTime Date { get; set; }
        public Pairing[] Pairings { get; set; }

    }
}