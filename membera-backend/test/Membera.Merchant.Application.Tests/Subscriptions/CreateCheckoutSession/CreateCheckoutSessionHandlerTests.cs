using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Subscriptions.CreateCheckoutSession;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.Subscriptions.CreateCheckoutSession;

public class CreateCheckoutSessionHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IUserSubscriptionRepository> _userSubscriptionRepositoryMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly Mock<ILogger<CreateCheckoutSessionHandler>> _loggerMock;
    private readonly CreateCheckoutSessionHandler _handler;

    public CreateCheckoutSessionHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _userSubscriptionRepositoryMock = new Mock<IUserSubscriptionRepository>();
        _stripeServiceMock = new Mock<IStripeService>();
        _loggerMock = new Mock<ILogger<CreateCheckoutSessionHandler>>();
        _handler = new CreateCheckoutSessionHandler(
            _subscriptionPlanRepositoryMock.Object,
            _userSubscriptionRepositoryMock.Object,
            _stripeServiceMock.Object,
            _loggerMock.Object);
    }

    private static CreateCheckoutSessionCommand BuildCommand(Guid planId) => new(
        Guid.NewGuid(),
        planId,
        "https://app.membera.test/success",
        "https://app.membera.test/cancel");

    private static SubscriptionPlan BuildPlan(Guid merchantId) => new(
        merchantId, "Gold", "Premium tier", 29.99m, 30, 100,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(command.SubscriptionPlanId))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _stripeServiceMock.Verify(
            s => s.CreateCheckoutSessionAsync(
                It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _userSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid());

        var plan = BuildPlan(Guid.NewGuid());
        plan.Deactivate();

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(command.SubscriptionPlanId))
            .ReturnsAsync(plan);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        Assert.Contains("not active", ex.Message);

        _stripeServiceMock.Verify(
            s => s.CreateCheckoutSessionAsync(
                It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _userSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserSubscription>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanFoundAndActive_CreatesCheckoutSessionAndSubscription()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid());

        var plan = BuildPlan(Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(command.SubscriptionPlanId))
            .ReturnsAsync(plan);

        _stripeServiceMock
            .Setup(s => s.CreateCheckoutSessionAsync(plan.Name, plan.Price, command.SuccessUrl, command.CancelUrl))
            .ReturnsAsync(("sess_123", "https://checkout.stripe.com/pay/sess_123"));

        UserSubscription? capturedSubscription = null;
        _userSubscriptionRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<UserSubscription>()))
            .Callback<UserSubscription>(s => capturedSubscription = s)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _stripeServiceMock.Verify(
            s => s.CreateCheckoutSessionAsync(plan.Name, plan.Price, command.SuccessUrl, command.CancelUrl),
            Times.Once);
        _userSubscriptionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserSubscription>()), Times.Once);

        Assert.NotNull(capturedSubscription);
        Assert.Equal(command.UserId, capturedSubscription!.UserId);
        Assert.Equal(command.SubscriptionPlanId, capturedSubscription.SubscriptionPlanId);
        Assert.Equal("sess_123", capturedSubscription.StripeSessionId);

        Assert.Equal("https://checkout.stripe.com/pay/sess_123", result.CheckoutUrl);
    }
}
