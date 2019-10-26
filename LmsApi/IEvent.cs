using System;

namespace Ojb500.EcfLms
{
    public interface IEvent
    {
        Team Away { get; }
        string Competition { get; }
        DateTime DateTime { get; }
        Team Home { get; }
        string MatchLink { get; }
        Score Result { get; }
    }
}