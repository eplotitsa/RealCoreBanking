namespace RealCoreBanking.Core.Events;

public interface IBankEvent
{
    DateTime OccurredAt { get; }
    string EventType { get; }
}
