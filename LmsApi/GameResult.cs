using System;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms
{
    public enum GameResultKind
    {
        Unplayed, // 0-0
        LeftWin, // 1-0
        Draw, // 1/2-1/2
        RightWin, // 0-1
    }

    public readonly struct GameResult
    {
        private readonly GameResultKind _result;
        private readonly bool _byDefault;

        public GameResult(GameResultKind result, bool byDefault = false)
        {
            _result = result;
            _byDefault = byDefault;
        }
        public override string ToString()
        {
            string result;
            switch (_result)
            {
                case GameResultKind.LeftWin: result = "1–0"; break;
                case GameResultKind.RightWin: result = "0–1"; break;
                case GameResultKind.Draw: result = "½–½"; break;
                default: result = "0–0"; break;
            }
            if (!_byDefault)
            {
                return result;
            }
            return $"{result} (def)";
        }

        private static Regex _trimmer = new Regex(@"[10½]");

        internal static GameResult Parse(string v)
        {
            var match = _trimmer.Matches(v);
            
            if (match.Count == 0)
            {
                return default;
            }
            if (match.Count != 2) {
                throw new InvalidOperationException("Invalid game result");
            }

            bool isDefault = v.Contains("def");
            GameResultKind grk;
            switch (match[0].Value)
            {
                case "1": grk = GameResultKind.LeftWin; break;
                case "½": grk = GameResultKind.Draw; break;
                case "0":
                    if (match[1].Value == "1")
                    {
                        grk = GameResultKind.RightWin;
                    }
                    else
                    {
                        grk = GameResultKind.Unplayed;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid game result");
            }

            return new GameResult(grk, isDefault);
        }
    }
}