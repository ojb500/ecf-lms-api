﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ojb500.EcfLms
{
    public struct Points
    {
        public int PointsX2 { get; set; }

        public static bool TryParse(ReadOnlySpan<char> readOnlySpan, out Points points, out int charsConsumed)
        {
            int pts = 0;
            bool half = false;
            bool any = false;
            int i = 0;
            for (i = 0; i < readOnlySpan.Length && char.IsWhiteSpace(readOnlySpan[i]); i++)
            {

            }

            for (; i < readOnlySpan.Length; i++)
            {
                char c = readOnlySpan[i];
                if (c == '½')
                {
                    any = true;
                    half = true;
                    break;
                }
                else if (c >= '0' && c <= '9')
                {
                    any = true;
                    int v = c - '0';
                    pts = (pts * 10) + v;
                }
                else
                {
                    i--;
                    break;
                }
            }


            if (!any)
            {
                charsConsumed = 0;
                points = default;
                return false;
            }

            charsConsumed = i + 1;
            pts = pts * 2;
            if (half)
            {
                pts++;
            }
            points = new Points(pts);
            return true;
        }

        public Points(string pts)
        {
            var result = Points.TryParse(pts.AsSpan(), out this, out _);
        }

        [JsonConstructor]
        public Points(int ptsx2)
        {
            PointsX2 = ptsx2;
        }

        public override string ToString()
        {
            bool half = (PointsX2 & 1) != 0;
            int pts = PointsX2 >> 1;
            if (half)
                return pts == 0 ? "½" : $"{pts}½";
            return pts.ToString();
        }


        public static Points operator -(Points a, Points b) => new Points(a.PointsX2 - b.PointsX2);
        public static Points operator +(Points a, Points b) => new Points(a.PointsX2 + b.PointsX2);
    }
}
