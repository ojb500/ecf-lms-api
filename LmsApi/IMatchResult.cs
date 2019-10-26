namespace Ojb500.EcfLms
{
    public interface IMatchResult : IEvent
    {
        Pairing[] Pairings { get; }
    }
}