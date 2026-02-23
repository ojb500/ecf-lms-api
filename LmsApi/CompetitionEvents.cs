namespace Ojb500.EcfLms
{
    /// <summary>A group of fixtures belonging to a single competition, as returned by the club endpoint.</summary>
    public class CompetitionEvents
    {
        internal CompetitionEvents(string title, Event[] events)
        {
            Title = title;
            Events = events;
        }

        /// <summary>The competition name (e.g. "Div 1 - Davy Trophy").</summary>
        public string Title { get; }

        /// <summary>The fixtures in this competition.</summary>
        public Event[] Events { get; }

        public override string ToString() => $"{Title} ({Events.Length} fixtures)";
    }
}
