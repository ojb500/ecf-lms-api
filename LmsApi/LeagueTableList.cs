using System.Collections;
using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    internal class LeagueTableList : ILeagueTable
    {
        public LeagueTableList(List<LeaguePosition> positions, string name)

        {
            Positions = positions;
            Name = name;
        }

        private List<LeaguePosition> Positions { get; }
        public string Name { get; }

        public IEnumerator<LeaguePosition> GetEnumerator()
        {
            return Positions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
