using Newtonsoft.Json;
using System;

namespace Ojb500.EcfLms
{
    public class Event : IEvent
    {
        public Event(Team home, string result, Team away, string matchLink, DateTime dt, Competition competition)
            :this(home, new Score(result), away, matchLink, dt, competition)
        {
            
        }

        [JsonConstructor]
        public Event(Team home, Score result, Team away, string matchLink, DateTime dateTime, Competition competition = null)
        {
            Home = home;
            Result = result;
            Away = away;
            MatchLink = matchLink;
            DateTime = dateTime;
            Competition = competition;
        }

        public Team Home { get; set; }
        public Score Result { get; set; }
        public Team Away { get; set; }
        public string MatchLink { get; set; }
        public DateTime DateTime { get; set; }

        [JsonIgnore]
        public Competition Competition { get; set; }
        public override string ToString() => $"{Home} {Result} {Away} ({DateTime})";
    }
}
