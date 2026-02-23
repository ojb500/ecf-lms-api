using System;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>A team match score (e.g. "4½–1½").</summary>
    public struct Score
    {
        public Points Home { get; }
        public Points Away { get; }

        [JsonIgnore]
        public bool IsEmpty => (Home + Away).PointsX2 == 0;
        public override string ToString() => $"{Home}–{Away}";

        public Score(Points h, Points a) => (Home, Away) = (h, a);
        public Score(string s)
        {
            var span = s.AsSpan();
            bool points1 = Points.TryParse(span, out var pts1, out var ix);
            if (!points1)
            {
                throw new InvalidOperationException();
            }
            bool points2 = Points.TryParse(span.Slice(ix + 2), out var pts2, out _);
            if (!points2)
            {
                throw new InvalidOperationException();
            }
            Home = pts1;
            Away = pts2;
        }
    }
}
