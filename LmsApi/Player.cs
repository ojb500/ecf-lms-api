using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ojb500.EcfLms
{
    public struct Rating
    {
        public int Primary { get; }
        public int? Secondary { get; }

        public Rating(int primary, int? secondary = null)
        {
            Primary = primary;
            Secondary = secondary;
        }

        public Rating(string value)
        {
            // New API format: "2081 (2031)" — current rating, then previous in parens.
            // "0000 (1751)" means no current rating. "0000 ()" means completely unrated.

            int? secondary = null;
            var parenOpen = value.IndexOf('(');
            if (parenOpen >= 0)
            {
                var parenClose = value.IndexOf(')', parenOpen);
                if (parenClose > parenOpen + 1)
                {
                    var inner = value.Substring(parenOpen + 1, parenClose - parenOpen - 1).Trim();
                    if (int.TryParse(inner, out var sec))
                        secondary = sec;
                }
                value = value.Substring(0, parenOpen);
            }

            value = value.Trim().TrimStart('0');
            var gr = Regex.Match(value, @"[0-9]+").Value;
            Primary = string.IsNullOrEmpty(gr) ? 0 : int.Parse(gr);
            Secondary = secondary;
        }

        public override string ToString() =>
            Primary == 0 && !Secondary.HasValue ? "ug" : Primary.ToString();
    }

    public struct Player
    {
        public Player(string name, Rating rating)
        {
            var lastFirst = name.Split(new char[] { ',' } , StringSplitOptions.RemoveEmptyEntries);
            for (int i=0; i < lastFirst.Length; i++)
            {
                lastFirst[i] = lastFirst[i].Trim(new char[] { '.', ' ' });
            }
            if (lastFirst.Length == 2)
            {
                GivenName = lastFirst[1];
                FamilyName = lastFirst[0];
            }
            else if (lastFirst.Length == 1)
            {
                GivenName = null;
                FamilyName = lastFirst[0];
            }
            else
            {
                throw new InvalidOperationException("Invalid player name");
            }
            Rating = rating;
        }

        public Player(string givenName, string familyName, Rating rating)
        {
            GivenName = givenName;
            FamilyName = familyName;
            Rating = rating;
        }
        public string GivenName { get; }
        public string FamilyName { get; }
        public Rating Rating { get; }
        public bool IsDefault => FamilyName == "Default" && string.IsNullOrEmpty(GivenName);
        public override string ToString()
        {
            if (GivenName == null)
            {
                return FamilyName;
            }
            return $"{FamilyName}, {GivenName} ({Rating})";
        }
    }
}
