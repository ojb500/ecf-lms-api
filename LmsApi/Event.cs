using Ojb500.EcfLms.Json;
using System;
using System.Text.Json.Serialization;

namespace Ojb500.EcfLms
{
    /// <summary>A scheduled or completed fixture between two teams.</summary>
	[JsonConverter(typeof(EventApiConverter))]
    public class Event
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

        /// <summary>
        /// The competition this event belongs to.
        /// Set when events are loaded via <see cref="Competition.GetEvents"/>.
        /// </summary>
        [JsonIgnore]
        public Competition Competition { get; internal set; }
        public override string ToString() => $"{Home} {Result} {Away} ({(DateTime.HasValue ? DateTime.Value.ToShortDateString() : "Postponed")})";
    }
}
