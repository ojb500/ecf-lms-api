using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;

namespace Ojb500.EcfLms.Json
{
    internal static class HtmlDeparse
    {
        private static Regex link = new Regex(@"(?:<a href=""([^""]+)"">)?([^<]+)(?:</a>)?");
        private static Regex tag = new Regex(@"<[^>]+>([^<]+)</[^>]+>");

        public static string StripTag(JsonNode token) => StripTag(token.AsValue().GetValue<string>());
        public static string StripTag(string s)
        {
            var match = tag.Match(s);
            return match.Success ? HttpUtility.HtmlDecode(match.Groups[1].Value) : s;
        }

        public static (string link, string text) DeparseLink(JsonNode token) => DeparseLink(token.AsValue().GetValue<string>());
        public static (string link, string text) DeparseLink(string s)
        {
            var match = link.Match(s);

            return (match.Groups[1].Value, HttpUtility.HtmlDecode(match.Groups[2].Value));
        }
    }
}
