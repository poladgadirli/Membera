using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.Merchants.UpdateMerchant;

public class UpdateMerchantHandlerTests
{
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly UpdateMerchantHandler _handler;

    public UpdateMerchantHandlerTests()
    {
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _handler = new UpdateMerchantHandler(_merchantRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new UpdateMerchantCommand(Guid.NewGuid(), "Updated Business", "Updated description");

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(command.OwnerId))
            .ReturnsAsync((MerchantEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _merchantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MerchantEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantFound_UpdatesProfileAndPersistsChanges()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateMerchantCommand(ownerId, "Polad's New Business", "A fresh description");

        var merchant = new MerchantEntity(ownerId, "Polad's Old Business");

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId))
            .ReturnsAsync(merchant);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(command.BusinessName, merchant.BusinessName);
        Assert.Equal(command.Description, merchant.Description);
        _merchantRepositoryMock.Verify(r => r.UpdateAsync(merchant), Times.Once);
    }
}
