using System;
using System.Linq;

namespace Ojb500.EcfLms
{
    public class MatchCard
    {
        public MatchCard()
        {

        }
        internal MatchCard(Team left, Team right, DateTime? date, Pairing[] pairings, Adjustment[] adjustments = null)
        {
            Left = left;
            Right = right;
            Date = date;
            Pairings = pairings;
            Adjustments = adjustments ?? Array.Empty<Adjustment>();
        }

        public Team Left { get; set; }
        public Team Right { get; set; }
        public DateTime? Date { get; set; }
        public Pairing[] Pairings { get; set; }
        public Adjustment[] Adjustments { get; set; } = Array.Empty<Adjustment>();
        public override string ToString()
        {
            var s = $"{Left.Abbreviated} v {Right.Abbreviated}";
            if (Date.HasValue)
                s += $", {Date.Value.ToShortDateString()}";
            if (Adjustments.Length > 0)
                s += $" [{string.Join(", ", Adjustments.Select(a => a.ToString()))}]";
            return s;
        }
    }

    public readonly struct Adjustment
    {
        public Score Score { get; }

        public Adjustment(Score score)
        {
            Score = score;
        }

        public override string ToString() => $"Adj {Score}";
    }
}
