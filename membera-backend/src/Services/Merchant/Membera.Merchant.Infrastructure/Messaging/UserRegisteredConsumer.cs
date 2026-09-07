using System.Text;
using System.Text.Json;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Shared.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Membera.Merchant.Infrastructure.Messaging;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private const string ExchangeName = "membera.events";
    private const string QueueName = "merchant.user-registered";
    private const string RoutingKey = "user.registered";

    public UserRegisteredConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var userRegisteredEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

            if (userRegisteredEvent is not null && userRegisteredEvent.Role == "MerchantOwner")
            {
                using var scope = _scopeFactory.CreateScope();
                var createMerchantHandler = scope.ServiceProvider.GetRequiredService<CreateMerchantHandler>();

                await createMerchantHandler.HandleAsync(
                    new CreateMerchantCommand(userRegisteredEvent.UserId, $"{userRegisteredEvent.FirstName}'s Business"));
            }

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, stoppingToken);
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }
}