using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    public interface ILeagueTable : IEnumerable<LeaguePosition>
    {
        string Name { get; }
    }

}
