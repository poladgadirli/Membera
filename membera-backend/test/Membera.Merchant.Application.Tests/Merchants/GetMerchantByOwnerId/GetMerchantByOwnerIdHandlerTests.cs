using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Shared.Caching;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.Merchants.GetMerchantByOwnerId;

public class GetMerchantByOwnerIdHandlerTests
{
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<GetMerchantByOwnerIdHandler>> _loggerMock;
    private readonly GetMerchantByOwnerIdHandler _handler;

    public GetMerchantByOwnerIdHandlerTests()
    {
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<GetMerchantByOwnerIdHandler>>();

        // Default: cache miss, so tests exercise the repository path unless they opt in.
        _cacheServiceMock
            .Setup(c => c.GetAsync<GetMerchantByOwnerIdResult>(It.IsAny<string>()))
            .ReturnsAsync((GetMerchantByOwnerIdResult?)null);

        _handler = new GetMerchantByOwnerIdHandler(
            _merchantRepositoryMock.Object, _cacheServiceMock.Object, _loggerMock.Object);
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

    [Fact]
    public async Task HandleAsync_WithCachedResult_ReturnsFromCacheWithoutHittingRepository()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var query = new GetMerchantByOwnerIdQuery(ownerId);
        var cacheKey = $"merchant:owner:{ownerId}";

        var cachedResult = new GetMerchantByOwnerIdResult(
            Guid.NewGuid(), ownerId, "Cached Business", "Cached description", null, true);

        _cacheServiceMock
            .Setup(c => c.GetAsync<GetMerchantByOwnerIdResult>(cacheKey))
            .ReturnsAsync(cachedResult);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Same(cachedResult, result);
        _merchantRepositoryMock.Verify(r => r.GetByOwnerIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnCacheMiss_CallsSetAsyncToPopulateCache()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var query = new GetMerchantByOwnerIdQuery(ownerId);
        var cacheKey = $"merchant:owner:{ownerId}";

        var merchant = new MerchantEntity(ownerId, "Polad's Coffee");
        merchant.UpdateProfile("Polad's Coffee", "Best coffee in town");

        _merchantRepositoryMock
            .Setup(r => r.GetByOwnerIdAsync(ownerId))
            .ReturnsAsync(merchant);

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _cacheServiceMock.Verify(
            c => c.SetAsync(
                cacheKey,
                It.Is<GetMerchantByOwnerIdResult>(r =>
                    r.Id == merchant.Id &&
                    r.OwnerId == merchant.OwnerId &&
                    r.BusinessName == merchant.BusinessName &&
                    r.Description == merchant.Description &&
                    r.IsActive == merchant.IsActive),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }
}
