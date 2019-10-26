using System;

namespace Ojb500.EcfLms
{
    public class Event : IEvent
    {
        public Event(Team home, string result, Team away, string matchLink, DateTime dt, Competition competition)
        {
            Home = home;
            Result = new Score(result);
            Away = away;
            MatchLink = matchLink;
            DateTime = dt;
            Competition = competition;
        }

        public Team Home { get; }
        public Score Result { get; }
        public Team Away { get; }
        public string MatchLink { get; }
        public DateTime DateTime { get; }
        public Competition Competition { get; set; }
        public override string ToString() => $"{Home} {Result} {Away} ({DateTime})";
    }
}
