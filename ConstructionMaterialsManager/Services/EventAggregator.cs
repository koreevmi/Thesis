namespace ConstructionMaterialsManager.Services;

/// <summary>
/// Простой Pub/Sub агрегатор для межкомпонентного взаимодействия.
/// Позволяет подписываться на события изменения данных и публиковать их.
/// </summary>
public interface IEventAggregator
{
    void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class;
    void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class;
    void Publish<TMessage>(TMessage message) where TMessage : class;
}

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _lock = new();

    public void Subscribe<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        lock (_lock)
        {
            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType))
                _subscribers[messageType] = new List<Delegate>();

            _subscribers[messageType].Add(handler);
        }
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler) where TMessage : class
    {
        lock (_lock)
        {
            var messageType = typeof(TMessage);
            if (_subscribers.ContainsKey(messageType))
                _subscribers[messageType].Remove(handler);
        }
    }

    public void Publish<TMessage>(TMessage message) where TMessage : class
    {
        lock (_lock)
        {
            var messageType = typeof(TMessage);
            if (!_subscribers.ContainsKey(messageType))
                return;

            foreach (var handler in _subscribers[messageType].ToList())
            {
                ((Action<TMessage>)handler)(message);
            }
        }
    }
}
