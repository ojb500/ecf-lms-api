using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ojb500.EcfLms
{
    public struct Score
    { 
        public Points Home { get; set; }
        public Points Away { get; set; }

        [JsonIgnore]
        public bool IsEmpty => (Home + Away).PointsX2 == 0;
        public override string ToString() => $"{Home}–{Away}";

        [JsonConstructor]
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
