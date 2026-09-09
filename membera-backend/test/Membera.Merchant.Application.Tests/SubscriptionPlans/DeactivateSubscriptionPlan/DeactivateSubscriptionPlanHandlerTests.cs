using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.DeactivateSubscriptionPlan;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.DeactivateSubscriptionPlan;

public class DeactivateSubscriptionPlanHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<DeactivateSubscriptionPlanHandler>> _loggerMock;
    private readonly DeactivateSubscriptionPlanHandler _handler;

    public DeactivateSubscriptionPlanHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<DeactivateSubscriptionPlanHandler>>();
        _handler = new DeactivateSubscriptionPlanHandler(
            _subscriptionPlanRepositoryMock.Object, _loggerMock.Object);
    }

    private static SubscriptionPlan BuildPlan(Guid merchantId) => new(
        merchantId, "Gold", "Premium tier", 29.99m, 30, 100,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DeactivateSubscriptionPlanCommand(Guid.NewGuid(), Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(command.PlanId))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanBelongsToDifferentMerchant_ThrowsInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = new DeactivateSubscriptionPlanCommand(planId, Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(BuildPlan(Guid.NewGuid()));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanAlreadyInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new DeactivateSubscriptionPlanCommand(planId, merchantId);

        var plan = BuildPlan(merchantId);
        plan.Deactivate();

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanActiveAndOwnedByMerchant_DeactivatesAndPersists()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = new DeactivateSubscriptionPlanCommand(planId, merchantId);

        var plan = BuildPlan(merchantId);

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.False(plan.IsActive);
        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
    }
}
