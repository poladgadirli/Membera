using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.VerifyEmail;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.VerifyEmail;

public class VerifyEmailHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOtpCodeRepository> _otpCodeRepositoryMock;
    private readonly Mock<ILogger<VerifyEmailHandler>> _loggerMock;
    private readonly VerifyEmailHandler _handler;

    public VerifyEmailHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _otpCodeRepositoryMock = new Mock<IOtpCodeRepository>();
        _loggerMock = new Mock<ILogger<VerifyEmailHandler>>();

        _handler = new VerifyEmailHandler(
            _userRepositoryMock.Object,
            _otpCodeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new VerifyEmailCommand(Guid.NewGuid(), "123456");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WhenCodeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var command = new VerifyEmailCommand(user.Id, "123456");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.EmailVerification, command.Code))
            .ReturnsAsync((OtpCode?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCodeExpired_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var otpCode = new OtpCode(user.Id, OtpPurpose.EmailVerification);
        var command = new VerifyEmailCommand(user.Id, otpCode.Code);

        typeof(OtpCode).GetProperty(nameof(OtpCode.ExpiresAt))!
            .SetValue(otpCode, DateTime.UtcNow.AddMinutes(-1));

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.EmailVerification, command.Code))
            .ReturnsAsync(otpCode);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public async Task HandleAsync_WhenCodeAlreadyUsed_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var otpCode = new OtpCode(user.Id, OtpPurpose.EmailVerification);
        otpCode.MarkAsUsed();
        var command = new VerifyEmailCommand(user.Id, otpCode.Code);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.EmailVerification, command.Code))
            .ReturnsAsync(otpCode);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithValidCode_MarksEmailVerifiedAndConsumesCode()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var otpCode = new OtpCode(user.Id, OtpPurpose.EmailVerification);
        var command = new VerifyEmailCommand(user.Id, otpCode.Code);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.EmailVerification, command.Code))
            .ReturnsAsync(otpCode);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.True(user.IsEmailVerified);
        Assert.True(otpCode.IsUsed);
        _otpCodeRepositoryMock.Verify(r => r.UpdateAsync(otpCode), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
