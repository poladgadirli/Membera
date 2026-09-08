using System.Text;
using System.Text.Json;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Shared.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Membera.Merchant.Infrastructure.Messaging;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private const string ExchangeName = "membera.events";
    private const string QueueName = "merchant.user-registered";
    private const string RoutingKey = "user.registered";

    public UserRegisteredConsumer(IServiceScopeFactory scopeFactory, ILogger<UserRegisteredConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: stoppingToken);

        _logger.LogInformation("UserRegisteredConsumer started listening on queue {QueueName} for routing key {RoutingKey}", QueueName, RoutingKey);

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

                var businessName = $"{userRegisteredEvent.FirstName}'s Business";

                await createMerchantHandler.HandleAsync(
                    new CreateMerchantCommand(userRegisteredEvent.UserId, businessName));

                _logger.LogInformation(
                    "Merchant profile created from UserRegisteredEvent. UserId: {UserId}, BusinessName: {BusinessName}",
                    userRegisteredEvent.UserId, businessName);
            }

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, stoppingToken);
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }
}