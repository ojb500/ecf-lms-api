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

        public Team Left { get; }
        public Team Right { get; }
        public DateTime Date { get; }
        public Pairing[] Pairings { get; }

    }
}