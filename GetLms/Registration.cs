using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ojb500.EcfLms;

namespace GetLms
{
    public class PlayerMatches : IReadOnlyCollection<MatchCard>
    {
        readonly static Comparer<MatchCard> _comparer = Comparer<MatchCard>.Create((a, b) => a.Date.GetValueOrDefault(new DateTime(3000,1,1)).CompareTo(b.Date.GetValueOrDefault(new DateTime(3000, 1, 1))));
        SortedSet<MatchCard> _played = new SortedSet<MatchCard>(_comparer);

        public int Count => ((IReadOnlyCollection<MatchCard>)_played).Count;

        public void Add(MatchCard match) => _played.Add(match);

        public IEnumerator<MatchCard> GetEnumerator()
        {
            return ((IReadOnlyCollection<MatchCard>)_played).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyCollection<MatchCard>)_played).GetEnumerator();
        }
    }

    public class Registration
    {
        private Competition _division;
        private Dictionary<string, Dictionary<string, PlayerMatches>> _stats = new Dictionary<string, Dictionary<string, PlayerMatches>>();
        public Registration(Competition d2, int nRegisteredBoards)
        {
            _division = d2;
            foreach (var match in d2.GetMatches().OrderBy(mc => mc.Date))
            {
                Add(match, false, nRegisteredBoards);
                Add(match, true, nRegisteredBoards);
            }
        }

        private void Add(MatchCard match, bool left, int nRegisteredBoards)
        {
            var club = left ? match.Left : match.Right;
            if (! _stats.TryGetValue(club.Name, out var players))
            {
                players = _stats[club.Name] = new Dictionary<string, PlayerMatches>();
            }
            foreach (var pairing in match.Pairings.Take(nRegisteredBoards))
            {
                Player player = left ? pairing.FirstPlayer : pairing.SecondPlayer;
                string name = $"{player.FamilyName}, {player.GivenName}";
                if (!player.IsDefault && !string.IsNullOrEmpty(player.FamilyName) && !string.IsNullOrEmpty(player.GivenName))
                {
                    if (!players.TryGetValue(name, out var played))
                    {
                        played = players[name] = new PlayerMatches();
                    }

                    played.Add(match);
                }
            }
        }

        public IEnumerable<(string Team, string[] Players)> GetRegisteredPlayers(int maxGamesBeforeRegistration = 3)
        {
            return _stats.Select(kvp => (Team: kvp.Key, Players: kvp.Value.Where(tup => tup.Value.Count > maxGamesBeforeRegistration).Select(tup => tup.Key).ToArray())).Where(kvp => kvp.Players.Any());
        }
        public void Print(string team, int maxGamesBeforeRegistration = 3)
        {
            Print(_stats[team], maxGamesBeforeRegistration);
        }
        public static void Print(Dictionary<string, PlayerMatches> _nPlayed, int maxGamesBeforeRegistration = 3)
        {
            foreach (var player in _nPlayed.OrderBy(kvp => kvp.Value.Count).ThenBy(kvp => kvp.Key))
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = player.Value.Count switch
                {
                    int n when n > maxGamesBeforeRegistration => ConsoleColor.Red,
                    int n when n == maxGamesBeforeRegistration => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                Console.WriteLine($"{player.Key}\t{player.Value.Count}\n\t\t{string.Join("\n\t\t", player.Value)}");

                Console.ForegroundColor = old;
            }
        }
    }
}
