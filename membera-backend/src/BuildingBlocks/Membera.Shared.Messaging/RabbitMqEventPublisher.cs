using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Membera.Shared.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ExchangeName = "membera.events";

    private RabbitMqEventPublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public static async Task<RabbitMqEventPublisher> CreateAsync(string hostName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        return new RabbitMqEventPublisher(connection, channel);
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string routingKey) where TEvent : class
    {
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: routingKey,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}