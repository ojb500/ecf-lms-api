using System;

namespace Ojb500.EcfLms
{
    /// <summary>
    /// A single round result in a Swiss-system crosstable, e.g. "1 (b22)" or " ½ ( HPB)".
    /// </summary>
    public readonly struct RoundResult
    {
        public Points Score { get; }
        /// <summary>Whether the player had white. Null for byes, defaults, and unplayed rounds.</summary>
        public bool? IsWhite { get; }
        public int OpponentNumber { get; }
        public bool IsPlayed { get; }
        public bool IsDefault { get; }
        public bool IsHalfPointBye { get; }

        public RoundResult(Points score, bool? isWhite, int opponentNumber, bool isPlayed, bool isDefault, bool isHalfPointBye)
        {
            Score = score;
            IsWhite = isWhite;
            OpponentNumber = opponentNumber;
            IsPlayed = isPlayed;
            IsDefault = isDefault;
            IsHalfPointBye = isHalfPointBye;
        }

        /// <summary>
        /// Parses a round result string such as "1 (b22)", " ½ (w4)", "- (  )", "1 ( def)", or " ½ ( HPB)".
        /// </summary>
        public static RoundResult Parse(string s)
        {
            var span = s.AsSpan();

            if (!Points.TryParse(span, out var score, out var consumed))
            {
                // Unplayed round marker "-"
                return default;
            }

            int parenStart = span.Slice(consumed).IndexOf('(');
            if (parenStart < 0)
                return new RoundResult(score, false, 0, true, false, false);

            var inner = span.Slice(consumed + parenStart + 1);
            int parenEnd = inner.IndexOf(')');
            if (parenEnd >= 0) inner = inner.Slice(0, parenEnd);
            inner = inner.Trim();

            if (inner.Length == 0)
                return new RoundResult(score, null, 0, false, false, false);

            if (inner.Equals("def".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return new RoundResult(score, null, 0, true, true, false);

            if (inner.Equals("HPB".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return new RoundResult(score, null, 0, true, false, true);

            bool isWhite = inner[0] == 'w';
            int opponent = 0;
            for (int i = 1; i < inner.Length; i++)
            {
                if (inner[i] >= '0' && inner[i] <= '9')
                    opponent = opponent * 10 + (inner[i] - '0');
            }

            return new RoundResult(score, isWhite, opponent, true, false, false);
        }

        public override string ToString()
        {
            if (!IsPlayed && Score.PointsX2 == 0) return "-";
            if (IsDefault) return $"{Score} (def)";
            if (IsHalfPointBye) return $"{Score} (HPB)";
            var colour = IsWhite == true ? "w" : "b";
            return $"{Score} ({colour}{OpponentNumber})";
        }
    }
}
