using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.Merchants.GetMerchantByOwnerId;

public class GetMerchantByOwnerIdHandlerTests
{
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly GetMerchantByOwnerIdHandler _handler;

    public GetMerchantByOwnerIdHandlerTests()
    {
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _handler = new GetMerchantByOwnerIdHandler(_merchantRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new GetMerchantByOwnerIdQuery(Guid.NewGuid());

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(query.OwnerId))
            .ReturnsAsync((MerchantEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_WhenMerchantFound_ReturnsResultWithAllFieldsMapped()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var query = new GetMerchantByOwnerIdQuery(ownerId);

        var merchant = new MerchantEntity(ownerId, "Polad's Coffee");
        merchant.UpdateProfile("Polad's Coffee", "Best coffee in town");
        merchant.Deactivate();

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId))
            .ReturnsAsync(merchant);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(merchant.Id, result.Id);
        Assert.Equal(merchant.OwnerId, result.OwnerId);
        Assert.Equal(merchant.BusinessName, result.BusinessName);
        Assert.Equal(merchant.Description, result.Description);
        Assert.Equal(merchant.IsActive, result.IsActive);
    }
}
