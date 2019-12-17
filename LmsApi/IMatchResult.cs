namespace Ojb500.EcfLms
{
    public interface IMatchResult : IEvent
    {
        bool IsDefault { get; }
        Pairing[] Pairings { get; }
    }
}