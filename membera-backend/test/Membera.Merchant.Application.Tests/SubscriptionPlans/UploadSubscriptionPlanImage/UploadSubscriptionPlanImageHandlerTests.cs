using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.SubscriptionPlans.UploadSubscriptionPlanImage;
using Membera.Merchant.Domain.Entities;
using Membera.Shared.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Merchant.Application.Tests.SubscriptionPlans.UploadSubscriptionPlanImage;

public class UploadSubscriptionPlanImageHandlerTests
{
    private readonly Mock<ISubscriptionPlanRepository> _subscriptionPlanRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<UploadSubscriptionPlanImageHandler>> _loggerMock;
    private readonly UploadSubscriptionPlanImageHandler _handler;

    public UploadSubscriptionPlanImageHandlerTests()
    {
        _subscriptionPlanRepositoryMock = new Mock<ISubscriptionPlanRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<UploadSubscriptionPlanImageHandler>>();
        _handler = new UploadSubscriptionPlanImageHandler(
            _subscriptionPlanRepositoryMock.Object, _fileStorageServiceMock.Object, _loggerMock.Object);
    }

    private static UploadSubscriptionPlanImageCommand BuildCommand(
        Guid planId, Guid merchantId, string contentType = "image/jpeg") =>
        new(planId, merchantId, new MemoryStream(new byte[] { 1, 2, 3 }), "plan.jpg", contentType);

    private static SubscriptionPlan BuildPlan(Guid merchantId) => new(
        merchantId, "Gold", "Premium tier", 29.99m, 30, 100,
        new TimeOnly(8, 0), new TimeOnly(20, 0));

    [Fact]
    public async Task HandleAsync_WhenPlanNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand(Guid.NewGuid(), Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(command.PlanId))
            .ReturnsAsync((SubscriptionPlan?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPlanBelongsToDifferentMerchant_ThrowsInvalidOperationException()
    {
        // Arrange
        var planId = Guid.NewGuid();
        var command = BuildCommand(planId, Guid.NewGuid());
        var plan = BuildPlan(Guid.NewGuid());

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));
        Assert.Contains("permission", exception.Message, StringComparison.OrdinalIgnoreCase);

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenContentTypeIsNotImage_ThrowsInvalidOperationException()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = BuildCommand(planId, merchantId, contentType: "text/plain");

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(BuildPlan(merchantId));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));
        Assert.Contains("image", exception.Message, StringComparison.OrdinalIgnoreCase);

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()),
            Times.Never);
        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<SubscriptionPlan>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_UploadsFileUpdatesPlanAndReturnsUrl()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var command = BuildCommand(planId, merchantId);
        var plan = BuildPlan(merchantId);
        const string expectedUrl = "http://localhost:9000/subscription-plan-images/some-object.jpg";

        _subscriptionPlanRepositoryMock
            .Setup(r => r.GetByIdAsync(planId))
            .ReturnsAsync(plan);
        _fileStorageServiceMock
            .Setup(s => s.UploadFileAsync("subscription-plan-images", It.IsAny<string>(), It.IsAny<Stream>(), command.ContentType))
            .ReturnsAsync((string _, string objectName, Stream _, string _) => objectName);
        _fileStorageServiceMock
            .Setup(s => s.GetFileUrl("subscription-plan-images", It.IsAny<string>()))
            .Returns(expectedUrl);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(expectedUrl, result);
        Assert.Equal(expectedUrl, plan.ImageUrl);

        _fileStorageServiceMock.Verify(
            s => s.UploadFileAsync("subscription-plan-images", It.IsAny<string>(), It.IsAny<Stream>(), command.ContentType),
            Times.Once);
        _subscriptionPlanRepositoryMock.Verify(r => r.UpdateAsync(plan), Times.Once);
    }
}
