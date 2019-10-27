using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    public interface IModel
    {
        LeagueTable GetTable(string org, string name);
        IEnumerable<Event> GetEvents(string org, string name);
        IEnumerable<MatchCard> GetMatchCards(string org, string name);
    }
}
