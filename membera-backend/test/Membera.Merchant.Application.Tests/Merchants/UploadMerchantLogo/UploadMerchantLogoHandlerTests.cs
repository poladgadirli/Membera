using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.UploadMerchantLogo;
using Membera.Shared.Caching;
using Membera.Shared.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MerchantEntity = Membera.Merchant.Domain.Entities.Merchant;

namespace Membera.Merchant.Application.Tests.Merchants.UploadMerchantLogo;

public class UploadMerchantLogoHandlerTests
{
    private readonly Mock<IMerchantRepository> _merchantRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<UploadMerchantLogoHandler>> _loggerMock;
    private readonly UploadMerchantLogoHandler _handler;

    public UploadMerchantLogoHandlerTests()
    {
        _merchantRepositoryMock = new Mock<IMerchantRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<UploadMerchantLogoHandler>>();
        _handler = new UploadMerchantLogoHandler(
            _merchantRepositoryMock.Object, _fileStorageServiceMock.Object,
            _cacheServiceMock.Object, _loggerMock.Object);
    }

    private static UploadMerchantLogoCommand BuildCommand(Guid merchantId, string contentType = "image/png") =>
        new(merchantId, new MemoryStream(new byte[] { 1, 2, 3 }), "logo.png", contentType);

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

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
        _merchantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MerchantEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenContentTypeIsNotImage_ThrowsInvalidOperationException()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var command = BuildCommand(merchantId, contentType: "application/pdf");

        _merchantRepositoryMock
            .Setup(r => r.GetByIdAsync(merchantId))
            .ReturnsAsync(new MerchantEntity(Guid.NewGuid(), "Polad's Business"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));
        Assert.Contains("image", exception.Message, StringComparison.OrdinalIgnoreCase);

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
        _merchantRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MerchantEntity>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_UploadsFileUpdatesMerchantAndReturnsUrl()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var command = BuildCommand(merchantId);
        var ownerId = Guid.NewGuid();
        var merchant = new MerchantEntity(ownerId, "Polad's Business");
        const string expectedUrl = "http://localhost:9000/merchant-logos/some-object.png";

        _merchantRepositoryMock
            .Setup(r => r.GetByIdAsync(merchantId))
            .ReturnsAsync(merchant);
        _fileStorageServiceMock
            .Setup(s => s.UploadFileAsync("merchant-logos", It.IsAny<string>(), It.IsAny<Stream>(), command.ContentType))
            .ReturnsAsync((string _, string objectName, Stream _, string _) => objectName);
        _fileStorageServiceMock
            .Setup(s => s.GetFileUrl("merchant-logos", It.IsAny<string>()))
            .Returns(expectedUrl);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedUrl, result);
        Assert.Equal(expectedUrl, merchant.LogoUrl);

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync("merchant-logos", It.IsAny<string>(), It.IsAny<Stream>(), command.ContentType),
            Times.Once);
        _merchantRepositoryMock.Verify(r => r.UpdateAsync(merchant), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"merchant:owner:{ownerId}"), Times.Once);
    }
}
