namespace RealCoreBanking.Core.Events;

public static class EventBus
{
    private static readonly List<Action<IBankEvent>> _handlers = new();

    public static void Subscribe(Action<IBankEvent> handler) => _handlers.Add(handler);
    public static void Unsubscribe(Action<IBankEvent> handler) => _handlers.Remove(handler);

    public static void Publish(IBankEvent @event)
    {
        foreach (var handler in _handlers.ToList())
            handler(@event);
    }
}
