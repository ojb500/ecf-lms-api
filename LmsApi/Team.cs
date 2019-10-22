using System.Linq;

namespace LmsApi
{
    public struct Team
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public Team(string s)
        {
            (Url, Name) = HtmlDeparse.DeparseLink(s);
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
