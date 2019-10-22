using System;

namespace LmsApi
{
    public class Event
    {
        public Event(Team home, string result, Team away, string matchLink, DateTime dt, string competition)
        {
            Home = home;
            Result = result.Trim();
            Away = away;
            MatchLink = matchLink;
            DateTime = dt;
            Competition = competition;
        }

        public Team Home { get; }
        public string Result { get; }
        public Team Away { get; }
        public string MatchLink { get; }
        public DateTime DateTime { get; }
        public string Competition { get; }
        public override string ToString() => $"{Home} {Result} {Away} ({DateTime})";
    }
}
