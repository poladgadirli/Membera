namespace Membera.Shared.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class;
}