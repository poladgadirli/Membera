using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.BrowseActivePlans;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.BrowseActivePlans;

public class BrowseActivePlansHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly Mock<ILogger<BrowseActivePlansHandler>> _loggerMock;
    private readonly BrowseActivePlansHandler _handler;

    public BrowseActivePlansHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _loggerMock = new Mock<ILogger<BrowseActivePlansHandler>>();
        _handler = new BrowseActivePlansHandler(
            _subscriptionPlanRepositoryMock.Object,
            _merchantRepositoryMock.Object,
            _loggerMock.Object);
    }

    private static SubscriptionPlan BuildPlan(Guid merchantId, string name = "Gold") => new(
        merchantId, name, "Premium tier", 29.99m, 30, 100,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenNoActivePlansExist_ReturnsResultWithEmptyPlansList()
    {
        // Arrange
        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((new List<SubscriptionPlan>(), 0));

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        Assert.NotNull(result.Plans);
        Assert.Empty(result.Plans);
        Assert.Equal(0, result.TotalCount);
        _merchantRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenActivePlansExist_MapsPlanFieldsAndOwningMerchantInfo()
    {
        // Arrange — two merchants, two plans.
        var merchantAId = Guid.NewGuid();
        var merchantBId = Guid.NewGuid();

        var merchantA = new MerchantEntity(Guid.NewGuid(), "Blue Bottle Coffee");
        merchantA.UpdateLogo("https://cdn.membera.test/logos/blue-bottle.png");
        var merchantB = new MerchantEntity(Guid.NewGuid(), "Still Point Studio");
        // merchantB has no logo.

        var planA = BuildPlan(merchantAId, "Daily Espresso");
        planA.UpdateImage("https://cdn.membera.test/plans/espresso.jpg");
        var planB = new SubscriptionPlan(
            merchantBId, "Unlimited Yoga", null, 89.00m, 30, null,
            new TimeOnly(6, 0), new TimeOnly(22, 0));

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((new List<SubscriptionPlan> { planA, planB }, 2));
        _merchantRepositoryMock.Setup(r => r.GetByIdAsync(merchantAId)).ReturnsAsync(merchantA);
        _merchantRepositoryMock.Setup(r => r.GetByIdAsync(merchantBId)).ReturnsAsync(merchantB);

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        Assert.Equal(2, result.Plans.Count);
        Assert.Equal(2, result.TotalCount);

        var summaryA = result.Plans.Single(p => p.Id == planA.Id);
        Assert.Equal(merchantAId, summaryA.MerchantId);
        Assert.Equal("Blue Bottle Coffee", summaryA.MerchantBusinessName);
        Assert.Equal("https://cdn.membera.test/logos/blue-bottle.png", summaryA.MerchantLogoUrl);
        Assert.Equal(planA.Name, summaryA.Name);
        Assert.Equal(planA.Description, summaryA.Description);
        Assert.Equal(planA.Price, summaryA.Price);
        Assert.Equal(planA.DurationInDays, summaryA.DurationInDays);
        Assert.Equal(planA.UsageLimit, summaryA.UsageLimit);
        Assert.Equal(planA.ActiveFrom, summaryA.ActiveFrom);
        Assert.Equal(planA.ActiveUntil, summaryA.ActiveUntil);
        Assert.Equal(planA.ImageUrl, summaryA.ImageUrl);
        Assert.True(summaryA.IsActive);

        var summaryB = result.Plans.Single(p => p.Id == planB.Id);
        Assert.Equal(merchantBId, summaryB.MerchantId);
        Assert.Equal("Still Point Studio", summaryB.MerchantBusinessName);
        Assert.Null(summaryB.MerchantLogoUrl);
        Assert.Null(summaryB.Description);
        Assert.Null(summaryB.UsageLimit);
        Assert.Null(summaryB.ImageUrl);
    }

    [Fact]
    public async Task HandleAsync_ExcludesInactivePlans()
    {
        // Arrange — one active, one deactivated. (The repository is expected to
        // filter, but the handler filters defensively too — this asserts that.)
        var merchantId = Guid.NewGuid();
        var merchant = new MerchantEntity(Guid.NewGuid(), "Greenhouse Kitchen");

        var activePlan = BuildPlan(merchantId, "Lunch Pass");
        var inactivePlan = BuildPlan(merchantId, "Retired Plan");
        inactivePlan.Deactivate();

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((new List<SubscriptionPlan> { activePlan, inactivePlan }, 2));
        _merchantRepositoryMock.Setup(r => r.GetByIdAsync(merchantId)).ReturnsAsync(merchant);

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        var summary = Assert.Single(result.Plans);
        Assert.Equal(activePlan.Id, summary.Id);
        Assert.DoesNotContain(result.Plans, p => p.Id == inactivePlan.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantMissing_MapsPlanWithEmptyMerchantName()
    {
        // Arrange — plan whose merchant lookup returns null (defensive path,
        // mirrors GetMySubscriptionsHandler's "plan?.Name ?? string.Empty").
        var merchantId = Guid.NewGuid();
        var plan = BuildPlan(merchantId);

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((new List<SubscriptionPlan> { plan }, 1));
        _merchantRepositoryMock
            .Setup(r => r.GetByIdAsync(merchantId))
            .ReturnsAsync((MerchantEntity?)null);

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        var summary = Assert.Single(result.Plans);
        Assert.Equal(string.Empty, summary.MerchantBusinessName);
        Assert.Null(summary.MerchantLogoUrl);
    }

    [Fact]
    public async Task HandleAsync_LooksUpEachMerchantOnce_EvenWithMultiplePlansFromSameMerchant()
    {
        // Arrange — two plans, same merchant.
        var merchantId = Guid.NewGuid();
        var merchant = new MerchantEntity(Guid.NewGuid(), "Blue Bottle Coffee");

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((new List<SubscriptionPlan>
            {
                BuildPlan(merchantId, "Plan One"),
                BuildPlan(merchantId, "Plan Two"),
            }, 2));
        _merchantRepositoryMock.Setup(r => r.GetByIdAsync(merchantId)).ReturnsAsync(merchant);

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        Assert.Equal(2, result.Plans.Count);
        _merchantRepositoryMock.Verify(r => r.GetByIdAsync(merchantId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTotalCountExceedsPageSize_ReturnsFullTotalCountWithOnlyThatPagesPlans()
    {
        // Arrange — 12 total active plans, but the repository only hands back the
        // 9 belonging to page 1; TotalCount still reflects all 12.
        var merchantId = Guid.NewGuid();
        var merchant = new MerchantEntity(Guid.NewGuid(), "Greenhouse Kitchen");

        var pageOfPlans = Enumerable.Range(1, 9)
            .Select(i => BuildPlan(merchantId, $"Plan {i}"))
            .ToList();

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetPagedActiveAsync(1, 9))
            .ReturnsAsync((pageOfPlans, 12));
        _merchantRepositoryMock.Setup(r => r.GetByIdAsync(merchantId)).ReturnsAsync(merchant);

        // Act
        var result = await _handler.HandleAsync(new BrowseActivePlansQuery());

        // Assert
        Assert.Equal(9, result.Plans.Count);
        Assert.Equal(12, result.TotalCount);
    }
}
