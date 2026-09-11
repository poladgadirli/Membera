using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Subscriptions.HandleStripeWebhook;
using Membera.Merchant.Domain.Entities;
using Membera.Merchant.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.Subscriptions.HandleStripeWebhook;

public class HandleStripeWebhookHandlerTests
{
    private readonly Mock<IUserSubscriptionRepository> _userSubscriptionRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<HandleStripeWebhookHandler>> _loggerMock;
    private readonly HandleStripeWebhookHandler _handler;

    public HandleStripeWebhookHandlerTests()
    {
        _userSubscriptionRepositoryMock = new Mock<IUserSubscriptionRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<HandleStripeWebhookHandler>>();
        _handler = new HandleStripeWebhookHandler(
            _userSubscriptionRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static SubscriptionPlan BuildPlan(int durationInDays, int? usageLimit) => new(
        Guid.NewGuid(), "Gold", "Premium tier", 29.99m, durationInDays, usageLimit,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenSubscriptionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new HandleStripeWebhookCommand("sess_unknown");

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByStripeSessionIdAsync(command.StripeSessionId))
            .ReturnsAsync((UserSubscription?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSubscriptionFound_ActivatesFromPlanAndPersists()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = new HandleStripeWebhookCommand("sess_123");

        var subscription = new UserSubscription(Guid.NewGuid(), planId, command.StripeSessionId);
        var plan = BuildPlan(durationInDays: 30, usageLimit: 100);

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByStripeSessionIdAsync(command.StripeSessionId))
            .ReturnsAsync(subscription);
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(subscription), Times.Once);
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(100, subscription.UsagesRemaining);
    }
}
