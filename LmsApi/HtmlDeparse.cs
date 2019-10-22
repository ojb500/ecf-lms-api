using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace LmsApi
{
    internal static class HtmlDeparse
    {
        private static Regex link = new Regex(@"<a href=""([^""]+)"">([^<]+)</a>");
        private static Regex tag = new Regex(@"<[^>]+>([^<]+)</[^>]+>");

        public static string StripTag(JToken token) => StripTag(token.Value<string>());
        public static string StripTag(string s)
        {
            var match = tag.Match(s);
            return HttpUtility.HtmlDecode(match.Groups[1].Value);
        }

        public static (string link, string text) DeparseLink(JToken token) => DeparseLink(token.Value<string>());
        public static (string link, string text) DeparseLink(string s)
        {
            var match = link.Match(s);

            return (match.Groups[1].Value, HttpUtility.HtmlDecode(match.Groups[2].Value));
        }
    }
}
