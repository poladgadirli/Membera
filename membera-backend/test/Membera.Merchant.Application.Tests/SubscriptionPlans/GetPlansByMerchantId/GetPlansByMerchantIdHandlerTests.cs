using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.GetPlansByMerchantId;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.GetPlansByMerchantId;

public class GetPlansByMerchantIdHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<ILogger<GetPlansByMerchantIdHandler>> _loggerMock;
    private readonly GetPlansByMerchantIdHandler _handler;

    public GetPlansByMerchantIdHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _loggerMock = new Mock<ILogger<GetPlansByMerchantIdHandler>>();
        _handler = new GetPlansByMerchantIdHandler(
            _subscriptionPlanRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenNoPlansExist_ReturnsResultWithEmptyPlansList()
    {
        // Arrange
        var query = new GetPlansByMerchantIdQuery(Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByMerchantIdAsync(query.MerchantId))
            .ReturnsAsync(new List<SubscriptionPlan>());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result.Plans);
        Assert.Empty(result.Plans);
    }

    [Fact]
    public async Task HandleAsync_WhenPlansExist_MapsEachEntityIntoSummary()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var query = new GetPlansByMerchantIdQuery(merchantId);

        var planA = new SubscriptionPlan(
            merchantId, "Silver", "Entry tier", 9.99m, 30, 50,
            new TimeOnly(8, 0), new TimeOnly(20, 0));
        var planB = new SubscriptionPlan(
            merchantId, "Gold", null, 29.99m, 90, null,
            new TimeOnly(0, 0), new TimeOnly(23, 59));
        planB.Deactivate();

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByMerchantIdAsync(merchantId))
            .ReturnsAsync(new List<SubscriptionPlan> { planA, planB });

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Plans.Count);

        AssertMapped(planA, result.Plans[0]);
        AssertMapped(planB, result.Plans[1]);
    }

    private static void AssertMapped(SubscriptionPlan entity, SubscriptionPlanSummary summary)
    {
        Assert.Equal(entity.Id, summary.Id);
        Assert.Equal(entity.Name, summary.Name);
        Assert.Equal(entity.Description, summary.Description);
        Assert.Equal(entity.Price, summary.Price);
        Assert.Equal(entity.DurationInDays, summary.DurationInDays);
        Assert.Equal(entity.UsageLimit, summary.UsageLimit);
        Assert.Equal(entity.ActiveFrom, summary.ActiveFrom);
        Assert.Equal(entity.ActiveUntil, summary.ActiveUntil);
        Assert.Equal(entity.IsActive, summary.IsActive);
    }
}
