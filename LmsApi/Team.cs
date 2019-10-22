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
