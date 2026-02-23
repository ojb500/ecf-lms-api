using Ojb500.EcfLms.Json;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>A single row in a league table: team record, board points for/against, and match points.</summary>
	[JsonConverter(typeof(LeagueTableEntryApiConverter))]
    public class LeagueTableEntry
    {
        public LeagueTableEntry(Team team, int p, int w, int d, int l, Points f, Points a, Points pts)
        {
            Team = team;
            P = p;
            W = w;
            D = d;
            L = l;
            F = f;
            A = a;
            Pts = pts;
        }

        public Team Team { get; }

        /// <summary>Matches played.</summary>
        public int P { get; }
        /// <summary>Matches won.</summary>
        public int W { get; }
        /// <summary>Matches drawn.</summary>
        public int D { get; }
        /// <summary>Matches lost.</summary>
        public int L { get; }
        /// <summary>Board points scored (for).</summary>
        public Points F { get; }
        /// <summary>Board points conceded (against).</summary>
        public Points A { get; }
        /// <summary>Match points.</summary>
        public Points Pts { get; }
        public override string ToString()
        {
            return $"{Team.Abbreviated} (P{P} W{W} D{D} L{L} BD{F - A})";
        }
    }
}
