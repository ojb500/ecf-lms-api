using Ojb500.EcfLms.Json;
using System;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
	[JsonConverter(typeof(EventApiConverter))]
    public class Event : IEvent
    {
        public Event(Team home, string result, Team away, string matchLink, DateTime? dt, Competition competition)
            :this(home, new Score(result), away, matchLink, dt, competition)
        {
            
        }

        public Event(Team home, Score result, Team away, string matchLink, DateTime? dateTime, Competition competition = null)
        {
            Home = home;
            Result = result;
            Away = away;
            MatchLink = matchLink;
            DateTime = dateTime;
            Competition = competition;
        }

        public Team Home { get; }
        public Score Result { get; }
        public Team Away { get; }
        public string MatchLink { get; }
        public DateTime? DateTime { get; }

        [JsonIgnore]
        public Competition Competition { get; internal set; }
        public override string ToString() => $"{Home} {Result} {Away} ({(DateTime.HasValue ? DateTime.Value.ToShortDateString() : "Postponed")})";
    }
}
