using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Subscriptions.RedeemSubscription;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.Subscriptions.RedeemSubscription;

public class RedeemSubscriptionHandlerTests
{
    private readonly Mock<IUserSubscriptionRepository> _userSubscriptionRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<RedeemSubscriptionHandler>> _loggerMock;
    private readonly RedeemSubscriptionHandler _handler;

    public RedeemSubscriptionHandlerTests()
    {
        _userSubscriptionRepositoryMock = new Mock<IUserSubscriptionRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<RedeemSubscriptionHandler>>();
        _handler = new RedeemSubscriptionHandler(
            _userSubscriptionRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _loggerMock.Object);
    }

    // A plan whose daily active window spans the entire day, so "now" is always inside it.
    // The handler resolves the plan through GetByIdAsync(subscription.SubscriptionPlanId),
    // which the mock keys off the planId Guid, so the plan's own entity Id is irrelevant.
    private static SubscriptionPlan BuildAlwaysActivePlan(Guid merchantId, int? usageLimit = 100)
        => BuildPlan(merchantId, new TimeOnly(0, 0), TimeOnly.MaxValue, usageLimit);

    private static SubscriptionPlan BuildPlan(
        Guid merchantId, TimeOnly activeFrom, TimeOnly activeUntil, int? usageLimit = 100)
        => new(merchantId, "Gold", "Premium tier", 29.99m, 30, usageLimit, activeFrom, activeUntil);

    private static UserSubscription BuildActiveSubscription(Guid planId, int? usageLimit)
    {
        var subscription = new UserSubscription(Guid.NewGuid(), planId, "sess_123");
        subscription.Activate(30, usageLimit);
        return subscription;
    }

    [Fact]
    public async Task HandleAsync_WhenRedemptionCodeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RedeemSubscriptionCommand("MBR-UNKNOWN", Guid.NewGuid());

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByRedemptionCodeAsync(command.RedemptionCode))
            .ReturnsAsync((UserSubscription?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        Assert.Equal("Invalid redemption code.", ex.Message);

        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanBelongsToDifferentMerchant_ThrowsInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var requestingMerchantId = Guid.NewGuid();
        var command = new RedeemSubscriptionCommand("MBR-ABC123", requestingMerchantId);

        var subscription = BuildActiveSubscription(planId, usageLimit: 5);
        var plan = BuildAlwaysActivePlan(merchantId: Guid.NewGuid());

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByRedemptionCodeAsync(command.RedemptionCode))
            .ReturnsAsync(subscription);
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        Assert.Contains("does not belong to your business", ex.Message);

        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOutsideActiveHours_ThrowsInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var command = new RedeemSubscriptionCommand("MBR-ABC123", merchantId);

        var subscription = BuildActiveSubscription(planId, usageLimit: 5);

        // Build a one-hour window that starts two hours from now, so the current
        // time can never fall inside it regardless of when the test runs.
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var plan = BuildPlan(merchantId, now.AddHours(2), now.AddHours(3));

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByRedemptionCodeAsync(command.RedemptionCode))
            .ReturnsAsync(subscription);
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        Assert.Contains("not active at this hour", ex.Message);

        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenValidAndUsagesRemain_RedeemsAndPersists()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var command = new RedeemSubscriptionCommand("MBR-ABC123", merchantId);

        var subscription = BuildActiveSubscription(planId, usageLimit: 5);
        var plan = BuildAlwaysActivePlan(merchantId);

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByRedemptionCodeAsync(command.RedemptionCode))
            .ReturnsAsync(subscription);
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(subscription), Times.Once);
        Assert.Equal(4, subscription.UsagesRemaining);

        Assert.Equal(subscription.Id, result.SubscriptionId);
        Assert.Equal(4, result.UsagesRemaining);
        Assert.Equal(plan.Name, result.PlanName);
    }

    [Fact]
    public async Task HandleAsync_WhenNoUsagesRemain_PropagatesEntityInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var command = new RedeemSubscriptionCommand("MBR-ABC123", merchantId);

        var subscription = BuildActiveSubscription(planId, usageLimit: 0);
        var plan = BuildAlwaysActivePlan(merchantId, usageLimit: 0);

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByRedemptionCodeAsync(command.RedemptionCode))
            .ReturnsAsync(subscription);
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userSubscriptionRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UserSubscription>()), Times.Never);
    }
}
