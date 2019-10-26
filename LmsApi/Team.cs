﻿using System.Linq;

namespace Ojb500.EcfLms
{
    public readonly struct Team
    {
        public string Name { get;}
        public string Url { get; }


        public Team(string name, string url)
        {
            Name = name;
            Url = url;
        }
        public static Team Parse(string s)
        {
            var (url, name) = Json.HtmlDeparse.DeparseLink(s);
            return new Team(name, url);
        }

        public override string ToString() => Abbreviated;

        public string Abbreviated
        {
            get
            {
                var words = Name.Split(' ');

                if (words.Length >= 3 && words[1] == "&")
                {
                    var abbrev = $"{words[0][0]}&{words[2][0]}";
                    if (words.Length == 3)
                    {
                        return abbrev;
                    }
                    if (words.Length == 4)
                    {
                        return $"{abbrev} {words[3]}";
                    }
                }

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Length > 6 && words[i][0] != '&')
                    {
                        words[i] = words[i].Substring(0, 4);
                    }
                }
                return string.Join(" ", words);
            }
        }
    }
}
