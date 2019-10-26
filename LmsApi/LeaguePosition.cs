namespace Ojb500.EcfLms
{
    public readonly struct LeaguePosition
    {
        internal LeaguePosition(int pos, LeagueTableEntry entry)
        {
            Position = pos;
            Entry = entry;
        }
        public int Position { get; }
        public LeagueTableEntry Entry { get; }
    }

}
