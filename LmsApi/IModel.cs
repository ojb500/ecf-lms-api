using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// Backend interface for ECF LMS data access.
    /// Use <see cref="Api"/> for the standard HTTP implementation.
    /// </summary>
    public interface IModel
    {
        Task<LeagueTable> GetTableAsync(string org, string name, CancellationToken ct = default);
        Task<Event[]> GetEventsAsync(string org, string name, CancellationToken ct = default);
        Task<MatchCard[]> GetMatchCardsAsync(string org, string name, CancellationToken ct = default);
        Task<CompetitionEvents[]> GetClubEventsAsync(string org, string clubCode, CancellationToken ct = default);
        Task<Dictionary<string, string>> GetSeasonsAsync(string org, CancellationToken ct = default);
        Task<Dictionary<string, SeasonWithEvents>> GetSeasonsWithEventsAsync(string org, CancellationToken ct = default);
        Task<Crosstable> GetCrosstableAsync(string org, string name, CancellationToken ct = default);
    }
}
