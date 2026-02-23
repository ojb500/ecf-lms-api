using Ojb500.EcfLms.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using System.Net.Http.Json;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// Client for the ECF League Management System REST API.
    /// Use <see cref="Default"/> for the standard production endpoint,
    /// or construct with a custom base address for testing.
    /// </summary>
    public class Api : IModel
    {
        /// <summary>Shared client pointing at the production ECF LMS endpoint.</summary>
        public static readonly Api Default = new Api();


        public Api() : this("https://lms.englishchess.org.uk/lms/lmsrest/league/")
        {
        }
        public Api(string baseAddress)
        {
            _baseAddress = baseAddress;
            _hc = CreateHttpClient(baseAddress);
        }

        internal Api(HttpMessageHandler handler)
        {
            _baseAddress = "https://lms.englishchess.org.uk/lms/lmsrest/league/";
            _hc = new HttpClient(handler)
            {
                BaseAddress = new Uri(_baseAddress)
            };
            _hc.DefaultRequestHeaders.Accept.Clear();
            _hc.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }


        private async Task<Stream> GetJson(string file, string org, string name, CancellationToken ct)
        {
            Trace.WriteLine($"Requesting {file} with org={org}, name='{name}'");
			var result = await _hc.PostAsync(file, JsonContent.Create(
				new
				{
					org = org,
					name = name
				}), ct).ConfigureAwait(false);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        }

        private async Task<Stream> GetJson(string file, string org, CancellationToken ct)
        {
            Trace.WriteLine($"Requesting {file} with org={org}");
            var result = await _hc.PostAsync(file, JsonContent.Create(
                new { org = org }), ct).ConfigureAwait(false);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        }

        private readonly string _baseAddress;
        private readonly HttpClient _hc;

        private HttpClient CreateHttpClient(string baseAddress)
        {
            var hc = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };

            hc.DefaultRequestHeaders.Accept.Clear();
            hc.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return hc;
        }
        private T Deserialise<T>(Stream s)
        {
            var str = ReadString(s);
			return JsonSerializer.Deserialize<T>(str);
        }

        private string ReadString(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        private async Task<T[]> GetAsync<T>(string file, string org, string name, CancellationToken ct)
        {
            return Deserialise<T[]>(await GetJson(file, org, name, ct).ConfigureAwait(false));
        }

        private async Task<T> GetDirectAsync<T>(string file, string org, CancellationToken ct)
        {
            return Deserialise<T>(await GetJson(file, org, ct).ConfigureAwait(false));
        }

        private async Task<T> GetOneAsync<T>(string file, string org, string name, CancellationToken ct)
        {
            var results = await GetAsync<T>(file, org, name, ct).ConfigureAwait(false);
            if (results.Length != 1)
            {
                if (results.Length > 1)
                {
                    throw new InvalidOperationException($"Expected 1 result, got {results.Length}");
                }
                return default;
            }
            return results[0];
        }

        async Task<LeagueTable> IModel.GetTableAsync(string org, string name, CancellationToken ct)
        {
            var stream = await GetJson("table", org, name, ct).ConfigureAwait(false);
            var json = ReadString(stream);

            // The table endpoint returns a different JSON shape for cup competitions:
            // header is an object (round numbers) rather than an array (column names).
            // Detect this and throw a clear error rather than a cryptic deserialization failure.
            using (var doc = JsonDocument.Parse(json))
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("header", out var header) &&
                        header.ValueKind == JsonValueKind.Object)
                    {
                        var title = element.TryGetProperty("title", out var t) ? t.GetString() : name;
                        throw new InvalidOperationException(
                            $"'{title}' is a cup competition and does not have a league table. " +
                            $"The table endpoint returns a knockout draw for this competition.");
                    }
                }
            }

            var results = JsonSerializer.Deserialize<ApiResult<LeagueTableEntry>[]>(json);
            if (results.Length > 1)
                throw new InvalidOperationException($"Expected 1 result, got {results.Length}");
            if (results.Length == 0)
                return default;
            var r = results[0];
            return new LeagueTable { Title = r.Title, Header = r.Header, Data = r.Data };
        }

        async Task<Event[]> IModel.GetEventsAsync(string org, string name, CancellationToken ct)
        {
            var s = await GetOneAsync<ApiResult<Event>>("event", org, name, ct).ConfigureAwait(false);
            return s.Data;
        }

        async Task<MatchCard[]> IModel.GetMatchCardsAsync(string org, string name, CancellationToken ct)
        {
            var s = await GetAsync<MatchApiResult>("match", org, name, ct).ConfigureAwait(false);
            return s.Select(m => ParseMatchCard(m)).ToArray();
        }

        async Task<CompetitionEvents[]> IModel.GetClubEventsAsync(string org, string clubCode, CancellationToken ct)
        {
            var results = await GetAsync<ApiResult<Event>>("club", org, clubCode, ct).ConfigureAwait(false);
            return results.Select(r => new CompetitionEvents(r.Title, r.Data)).ToArray();
        }

        async Task<Dictionary<string, string>> IModel.GetSeasonsAsync(string org, CancellationToken ct)
        {
            return await GetDirectAsync<Dictionary<string, string>>("seasons", org, ct).ConfigureAwait(false);
        }

        async Task<Dictionary<string, SeasonWithEvents>> IModel.GetSeasonsWithEventsAsync(string org, CancellationToken ct)
        {
            return await GetDirectAsync<Dictionary<string, SeasonWithEvents>>("seasonsWithEvents", org, ct).ConfigureAwait(false);
        }

        public async Task<string> GetStandingsAsync(string org, string name, string season = null, CancellationToken ct = default)
        {
            Stream s;
            if (season != null)
            {
                Trace.WriteLine($"Requesting standings with org={org}, name='{name}', season='{season}'");
                var result = await _hc.PostAsync("standings", JsonContent.Create(
                    new { org = org, name = name, season = season }), ct).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                s = await result.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            }
            else
            {
                s = await GetJson("standings", org, name, ct).ConfigureAwait(false);
            }
            return ReadString(s);
        }

        internal static MatchCard ParseMatchCard(MatchApiResult m)
        {
            // Header: ["Board", "Rating", "HomeTeam", " V ", "AwayTeam", "Rating"]
            var pairings = new List<Pairing>();
            var adjustments = new List<Adjustment>();

            foreach (var el in m.Data)
            {
                if (el.ValueKind == JsonValueKind.Object)
                {
                    pairings.Add(ParsePairing(el));
                }
                else if (el.ValueKind == JsonValueKind.Array && el.GetArrayLength() > 3)
                {
                    // Adjustment rows: ["Adjustment", " ", " ", "3 : -3", " ", " "]
                    var adjStr = el[3].GetString();
                    if (!string.IsNullOrWhiteSpace(adjStr) && adjStr.Contains(':'))
                    {
                        var parts = adjStr.Split(':');
                        if (parts.Length == 2 &&
                            Points.TryParse(parts[0].AsSpan(), out var home, out _) &&
                            Points.TryParse(parts[1].AsSpan(), out var away, out _))
                        {
                            adjustments.Add(new Adjustment(new Score(home, away)));
                        }
                    }
                }
            }

            return new MatchCard(Team.Parse(m.Header[2]), Team.Parse(m.Header[4]),
                null,
                pairings.ToArray(),
                adjustments.ToArray());
        }

        private static readonly System.Text.RegularExpressions.Regex BoardRegex =
            new System.Text.RegularExpressions.Regex(@"(\d+)\s*\(\s*([WB])\s*\)");

        private static Pairing ParsePairing(JsonElement el)
        {
            var boardStr = el.GetProperty("board").GetString() ?? "";
            var boardMatch = BoardRegex.Match(boardStr);
            int board = boardMatch.Success ? int.Parse(boardMatch.Groups[1].Value) : 0;
            bool isWhite = boardMatch.Success && boardMatch.Groups[2].Value == "W";

            var hname = el.GetProperty("hname").GetString() ?? "";
            var hrating = el.GetProperty("hrating").GetString() ?? "";
            var aname = el.GetProperty("aname").GetString() ?? "";
            var arating = el.GetProperty("arating").GetString() ?? "";
            var result = el.GetProperty("result").GetString() ?? "";

            return new Pairing(board, isWhite,
                new Player(hname, new Rating(hrating)),
                new Player(aname, new Rating(arating)),
                GameResult.Parse(result));
        }
    }
}
