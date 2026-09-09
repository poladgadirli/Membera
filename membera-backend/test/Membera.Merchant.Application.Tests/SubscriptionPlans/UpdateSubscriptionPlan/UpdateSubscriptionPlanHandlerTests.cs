using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.UpdateSubscriptionPlan;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<UpdateSubscriptionPlanHandler>> _loggerMock;
    private readonly UpdateSubscriptionPlanHandler _handler;

    public UpdateSubscriptionPlanHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<UpdateSubscriptionPlanHandler>>();
        _handler = new UpdateSubscriptionPlanHandler(
            _subscriptionPlanRepositoryMock.Object, _loggerMock.Object);
    }

    private static UpdateSubscriptionPlanCommand BuildCommand(Guid planId, Guid merchantId) => new(
        planId,
        merchantId,
        "Updated Plan",
        "Updated description",
        99.99m,
        60,
        200,
        new TimeOnly(7, 30),
        new TimeOnly(21, 30));

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid(), Guid.NewGuid());

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
        var command = BuildCommand(planId, Guid.NewGuid());

        var plan = new SubscriptionPlan(
            Guid.NewGuid(), "Original", "Original description", 10m, 30, 10,
            new TimeOnly(8, 0), new TimeOnly(20, 0));

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));
        Assert.Contains("permission", exception.Message, StringComparison.OrdinalIgnoreCase);

        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanBelongsToCorrectMerchant_UpdatesDetailsAndPersists()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = BuildCommand(planId, merchantId);

        var plan = new SubscriptionPlan(
            merchantId, "Original", "Original description", 10m, 30, 10,
            new TimeOnly(8, 0), new TimeOnly(20, 0));

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(command.Name, plan.Name);
        Assert.Equal(command.Description, plan.Description);
        Assert.Equal(command.Price, plan.Price);
        Assert.Equal(command.DurationInDays, plan.DurationInDays);
        Assert.Equal(command.UsageLimit, plan.UsageLimit);
        Assert.Equal(command.ActiveFrom, plan.ActiveFrom);
        Assert.Equal(command.ActiveUntil, plan.ActiveUntil);

        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
    }
}
