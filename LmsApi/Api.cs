using Ojb500.EcfLms.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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


        private async Task<Stream> GetJson(string file, string org, string name)
        {
            Trace.WriteLine($"Requesting {file} with org={org}, name='{name}'");
			var result = await _hc.PostAsync(file, JsonContent.Create(
				new
				{
					org = org,
					name = name
				})).ConfigureAwait(false);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private async Task<Stream> GetJson(string file, string org)
        {
            Trace.WriteLine($"Requesting {file} with org={org}");
            var result = await _hc.PostAsync(file, JsonContent.Create(
                new { org = org })).ConfigureAwait(false);

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
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

        private T[] Get<T>(string file, string org, string name)
        {
            return Deserialise<T[]>(GetJson(file, org, name).Result);
        }

        private T GetDirect<T>(string file, string org)
        {
            return Deserialise<T>(GetJson(file, org).Result);
        }

        private T GetOne<T>(string file, string org, string name)
        {
            var results = Get<T>(file, org, name);
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

        LeagueTable IModel.GetTable(string org, string name)
        {
            var results = Get<ApiResult<LeagueTableEntry>>("table", org, name);
            if (results.Length > 1)
                throw new InvalidOperationException($"Expected 1 result, got {results.Length}");
            if (results.Length == 0)
                return default;
            var r = results[0];
            return new LeagueTable { Title = r.Title, Header = r.Header, Data = r.Data };
        }

        IEnumerable<Event> IModel.GetEvents(string org, string name)
        {
            var s = GetOne<ApiResult<Event>>("event", org, name);
            return s.Data;
        }

        IEnumerable<MatchCard> IModel.GetMatchCards(string org, string name)
        {
            var s = Get<MatchApiResult>("match", org, name);
            foreach (var m in s)
            {
                yield return ParseMatchCard(m);
            }
        }

        IEnumerable<CompetitionEvents> IModel.GetClubEvents(string org, string clubCode)
        {
            var results = Get<ApiResult<Event>>("club", org, clubCode);
            foreach (var r in results)
            {
                yield return new CompetitionEvents(r.Title, r.Data);
            }
        }

        Dictionary<string, string> IModel.GetSeasons(string org)
        {
            return GetDirect<Dictionary<string, string>>("seasons", org);
        }

        Dictionary<string, SeasonWithEvents> IModel.GetSeasonsWithEvents(string org)
        {
            return GetDirect<Dictionary<string, SeasonWithEvents>>("seasonsWithEvents", org);
        }

        public string GetStandings(string org, string name, string season = null)
        {
            Stream s;
            if (season != null)
            {
                Trace.WriteLine($"Requesting standings with org={org}, name='{name}', season='{season}'");
                var result = _hc.PostAsync("standings", JsonContent.Create(
                    new { org = org, name = name, season = season })).Result;
                result.EnsureSuccessStatusCode();
                s = result.Content.ReadAsStreamAsync().Result;
            }
            else
            {
                s = GetJson("standings", org, name).Result;
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
