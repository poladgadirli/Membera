using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.Merchants.CreateMerchant;

public class CreateMerchantHandlerTests
{
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly Mock<ILogger<CreateMerchantHandler>> _loggerMock;
    private readonly CreateMerchantHandler _handler;

    public CreateMerchantHandlerTests()
    {
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _loggerMock = new Mock<ILogger<CreateMerchantHandler>>();
        _handler = new CreateMerchantHandler(_merchantRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenOwnerAlreadyHasMerchantProfile_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreateMerchantCommand(Guid.NewGuid(), "New Business");

        var existingMerchant = new MerchantEntity(command.OwnerId, "Existing Business");

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(command.OwnerId))
            .ReturnsAsync(existingMerchant);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _merchantRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MerchantEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOwnerHasNoExistingProfile_CreatesMerchantSuccessfully()
    {
        // Arrange
        var command = new CreateMerchantCommand(Guid.NewGuid(), "Polad's Coffee");

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(command.OwnerId))
            .ReturnsAsync((MerchantEntity?)null);

        MerchantEntity? capturedMerchant = null;
        _merchantRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MerchantEntity>()))
            .Callback<MerchantEntity>(m => capturedMerchant = m)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        _merchantRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MerchantEntity>()), Times.Once);

        Assert.NotNull(capturedMerchant);
        Assert.Equal(command.OwnerId, capturedMerchant!.OwnerId);
        Assert.Equal(command.BusinessName, capturedMerchant.BusinessName);
        Assert.Equal(BusinessCategory.Other, capturedMerchant.Category);

        Assert.Equal(capturedMerchant.Id, result.Id);
        Assert.Equal(command.BusinessName, result.BusinessName);
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryProvided_CreatesMerchantWithThatCategory()
    {
        // Arrange
        var command = new CreateMerchantCommand(Guid.NewGuid(), "Polad's Gym", BusinessCategory.Gym);

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(command.OwnerId))
            .ReturnsAsync((MerchantEntity?)null);

        MerchantEntity? capturedMerchant = null;
        _merchantRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MerchantEntity>()))
            .Callback<MerchantEntity>(m => capturedMerchant = m)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(BusinessCategory.Gym, capturedMerchant!.Category);
    }
}
