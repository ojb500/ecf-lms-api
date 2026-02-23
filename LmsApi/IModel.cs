using System.Collections.Generic;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// Backend interface for ECF LMS data access.
    /// Use <see cref="Api"/> for the standard HTTP implementation.
    /// </summary>
    public interface IModel
    {
        LeagueTable GetTable(string org, string name);
        IEnumerable<Event> GetEvents(string org, string name);
        IEnumerable<MatchCard> GetMatchCards(string org, string name);
        IEnumerable<CompetitionEvents> GetClubEvents(string org, string clubCode);
        Dictionary<string, string> GetSeasons(string org);
        Dictionary<string, SeasonWithEvents> GetSeasonsWithEvents(string org);
    }
}
