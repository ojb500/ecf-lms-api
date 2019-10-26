using System;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms
{
    public readonly struct GameResult
    {
        public Result Result => _result;
        public bool WasDefaulted => _byDefault;

        private readonly Result _result;
        private readonly bool _byDefault;

        public GameResult(Result result, bool byDefault = false)
        {
            _result = result;
            _byDefault = byDefault;
        }
        public override string ToString()
        {
            string result;
            switch (_result)
            {
                case Result.LeftWin: result = "1–0"; break;
                case Result.RightWin: result = "0–1"; break;
                case Result.Draw: result = "½–½"; break;
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
            Result grk;
            switch (match[0].Value)
            {
                case "1": grk = Result.LeftWin; break;
                case "½": grk = Result.Draw; break;
                case "0":
                    if (match[1].Value == "1")
                    {
                        grk = Result.RightWin;
                    }
                    else
                    {
                        grk = Result.Unplayed;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid game result");
            }

            return new GameResult(grk, isDefault);
        }
    }
}