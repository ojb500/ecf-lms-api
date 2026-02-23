using System;
using System.Linq;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// A detailed match card showing board-by-board pairings between two teams,
    /// plus any score adjustments.
    /// </summary>
    public class MatchCard
    {
        internal MatchCard(Team left, Team right, DateTime? date, Pairing[] pairings, Adjustment[] adjustments = null)
        {
            Left = left;
            Right = right;
            Date = date;
            Pairings = pairings;
            Adjustments = adjustments ?? Array.Empty<Adjustment>();
        }

        public Team Left { get; }
        public Team Right { get; }
        public DateTime? Date { get; }
        public Pairing[] Pairings { get; }
        public Adjustment[] Adjustments { get; }
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

    /// <summary>A score adjustment applied to a match (e.g. penalty points).</summary>
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
