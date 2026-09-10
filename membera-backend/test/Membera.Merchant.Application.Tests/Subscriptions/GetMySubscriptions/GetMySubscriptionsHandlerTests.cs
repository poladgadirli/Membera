using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Subscriptions.GetMySubscriptions;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.Subscriptions.GetMySubscriptions;

public class GetMySubscriptionsHandlerTests
{
    private readonly Mock<IUserSubscriptionRepository> _userSubscriptionRepositoryMock;
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<GetMySubscriptionsHandler>> _loggerMock;
    private readonly GetMySubscriptionsHandler _handler;

    public GetMySubscriptionsHandlerTests()
    {
        _userSubscriptionRepositoryMock = new Mock<IUserSubscriptionRepository>();
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<GetMySubscriptionsHandler>>();
        _handler = new GetMySubscriptionsHandler(
            _userSubscriptionRepositoryMock.Object,
            _subscriptionPlanRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static SubscriptionPlan BuildPlan(string name) => new(
        Guid.NewGuid(), name, "Tier", 19.99m, 30, 100,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenNoSubscriptionsExist_ReturnsResultWithEmptyList()
    {
        // Arrange
        var query = new GetMySubscriptionsQuery(Guid.NewGuid());

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByUserIdAsync(query.UserId))
            .ReturnsAsync(new List<UserSubscription>());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result.Subscriptions);
        Assert.Empty(result.Subscriptions);
    }

    [Fact]
    public async Task HandleAsync_WhenSubscriptionsExist_MapsEachOneAndResolvesPlanName()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMySubscriptionsQuery(userId);

        var planIdA = Guid.NewGuid();
        var planIdB = Guid.NewGuid();

        var subA = new UserSubscription(userId, planIdA, "sess_a");
        subA.Activate(30, 10); // Status -> Active

        var subB = new UserSubscription(userId, planIdB, "sess_b"); // Status stays Pending

        _userSubscriptionRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<UserSubscription> { subA, subB });
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planIdA))
            .ReturnsAsync(BuildPlan("Gold"));
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planIdB))
            .ReturnsAsync(BuildPlan("Silver"));

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Subscriptions.Count);

        var summaryA = result.Subscriptions[0];
        Assert.Equal(subA.Id, summaryA.Id);
        Assert.Equal(planIdA, summaryA.SubscriptionPlanId);
        Assert.Equal("Gold", summaryA.PlanName);
        Assert.Equal(subA.RedemptionCode, summaryA.RedemptionCode);
        Assert.Equal(subA.StartedAt, summaryA.StartedAt);
        Assert.Equal(subA.ExpiresAt, summaryA.ExpiresAt);
        Assert.Equal(subA.UsagesRemaining, summaryA.UsagesRemaining);
        Assert.Equal("Active", summaryA.Status);
        Assert.Equal("sess_a", summaryA.StripeSessionId);

        var summaryB = result.Subscriptions[1];
        Assert.Equal(subB.Id, summaryB.Id);
        Assert.Equal(planIdB, summaryB.SubscriptionPlanId);
        Assert.Equal("Silver", summaryB.PlanName);
        Assert.Equal("Pending", summaryB.Status);
        Assert.Equal("sess_b", summaryB.StripeSessionId);
    }
}
