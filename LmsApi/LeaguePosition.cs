namespace Ojb500.EcfLms
{
    /// <summary>A team's position in a league table, combining rank with the table entry data.</summary>
    public readonly struct LeaguePosition
    {
        internal LeaguePosition(int pos, LeagueTableEntry entry)
        {
            Position = pos;
            Entry = entry;
        }

        /// <summary>The 1-based position in the table.</summary>
        public int Position { get; }
        public LeagueTableEntry Entry { get; }
    }
}
