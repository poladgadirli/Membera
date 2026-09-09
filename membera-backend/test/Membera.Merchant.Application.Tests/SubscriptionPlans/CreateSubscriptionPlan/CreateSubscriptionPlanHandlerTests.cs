using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;
using Membera.Merchant.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.CreateSubscriptionPlan;

public class CreateSubscriptionPlanHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly Mock<ILogger<CreateSubscriptionPlanHandler>> _loggerMock;
    private readonly CreateSubscriptionPlanHandler _handler;

    public CreateSubscriptionPlanHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _loggerMock = new Mock<ILogger<CreateSubscriptionPlanHandler>>();
        _handler = new CreateSubscriptionPlanHandler(
            _subscriptionPlanRepositoryMock.Object, _merchantRepositoryMock.Object, _loggerMock.Object);
    }

    private static CreateSubscriptionPlanCommand BuildCommand(Guid merchantId) => new(
        merchantId,
        "Gold Plan",
        "Premium tier",
        49.99m,
        30,
        100,
        new TimeOnly(8, 0),
        new TimeOnly(22, 0));

    [Fact]
    public async Task HandleAsync_WhenMerchantNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid());

        _merchantRepositoryMock
            .Setup(r => r.GetByIdAsync(command.MerchantId))
            .ReturnsAsync((MerchantEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _subscriptionPlanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantFound_CreatesPlanSuccessfully()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var command = BuildCommand(merchantId);

        _merchantRepositoryMock
            .Setup(r => r.GetByIdAsync(merchantId))
            .ReturnsAsync(new MerchantEntity(Guid.NewGuid(), "Polad's Coffee"));

        SubscriptionPlan? capturedPlan = null;
        _subscriptionPlanRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<SubscriptionPlan>()))
            .Callback<SubscriptionPlan>(p => capturedPlan = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _subscriptionPlanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SubscriptionPlan>()), Times.Once);

        Assert.NotNull(capturedPlan);
        Assert.Equal(command.MerchantId, capturedPlan!.MerchantId);
        Assert.Equal(command.Name, capturedPlan.Name);
        Assert.Equal(command.Price, capturedPlan.Price);

        Assert.Equal(capturedPlan.Id, result.Id);
        Assert.Equal(capturedPlan.Name, result.Name);
        Assert.Equal(capturedPlan.Price, result.Price);
    }
}
