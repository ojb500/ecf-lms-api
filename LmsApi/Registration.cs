using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ojb500.EcfLms;

namespace GetLms
{
    public class Registration
    {
        private Competition d2;

        public Registration(Competition d2)
        {
            Dictionary<string, List<MatchCard>> _nPlayed = new Dictionary<string, List<MatchCard>>();

            foreach (var match in d2.GetMatches().OrderBy(mc => mc.Date))
            {
                bool left;
                if (match.Left.Name.StartsWith("Roth"))
                {
                    left = true;
                }
                else if (match.Right.Name.StartsWith("Roth"))
                {
                    left = false;
                }
                else
                {
                    continue;
                }

                foreach (var pairing in match.Pairings.Take(3))
                {
                    Player player = left ? pairing.FirstPlayer : pairing.SecondPlayer;
                    string name = player.FamilyName;
                    if (!player.IsDefault && !string.IsNullOrEmpty(name))
                    {
                        List<MatchCard> played;

                        if (!_nPlayed.TryGetValue(name, out played))
                        {
                            played = _nPlayed[name] = new List<MatchCard>();
                        }

                        played.Add(match);
                    }
                }
            }
            foreach (var player in _nPlayed.OrderBy(kvp => kvp.Value.Count).ThenBy(kvp => kvp.Key))
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = player.Value.Count switch
                {
                    int n when n > 3 => ConsoleColor.Red,
                    int n when n == 3 => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green
                };
                Console.WriteLine($"{player.Key}\t{player.Value.Count}\n\t\t{string.Join("\n\t\t", player.Value)}");

                Console.ForegroundColor = old;
            }
        }
    }
}
