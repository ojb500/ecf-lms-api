using Ojb500.EcfLms.Json;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
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
        public int P { get; }
        public int W { get; }
        public int D { get; }
        public int L { get; }
        public Points F { get; }
        public Points A { get; }
        public Points Pts { get; }
        public override string ToString()
        {
            return $"{Team.Abbreviated} (P{P} W{W} D{D} L{L} BD{F - A})";
        }
    }
}
