using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.ResetPassword;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.ResetPassword;

public class ResetPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOtpCodeRepository> _otpCodeRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ILogger<ResetPasswordHandler>> _loggerMock;
    private readonly ResetPasswordHandler _handler;

    public ResetPasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _otpCodeRepositoryMock = new Mock<IOtpCodeRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _loggerMock = new Mock<ILogger<ResetPasswordHandler>>();

        _handler = new ResetPasswordHandler(
            _userRepositoryMock.Object,
            _otpCodeRepositoryMock.Object,
            _passwordHasherMock.Object,
            _refreshTokenRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationExceptionWithGenericMessage()
    {
        // Arrange
        var command = new ResetPasswordCommand("nobody@test.com", "123456", "NewPass123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
        Assert.Equal("Invalid or expired code.", ex.Message);

        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCodeNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "old-hash", UserRole.User);
        var command = new ResetPasswordCommand(user.Email, "123456", "NewPass123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.PasswordReset, command.Code))
            .ReturnsAsync((OtpCode?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCodeAlreadyUsed_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "old-hash", UserRole.User);
        var otpCode = new OtpCode(user.Id, OtpPurpose.PasswordReset);
        otpCode.MarkAsUsed();
        var command = new ResetPasswordCommand(user.Email, otpCode.Code, "NewPass123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.PasswordReset, command.Code))
            .ReturnsAsync(otpCode);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        Assert.Equal("old-hash", user.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_WithValidCode_ChangesPasswordConsumesCodeAndRevokesRefreshTokens()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "old-hash", UserRole.User);
        var otpCode = new OtpCode(user.Id, OtpPurpose.PasswordReset);
        var command = new ResetPasswordCommand(user.Email, otpCode.Code, "NewPass123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _otpCodeRepositoryMock
            .Setup(r => r.GetLatestAsync(user.Id, OtpPurpose.PasswordReset, command.Code))
            .ReturnsAsync(otpCode);

        _passwordHasherMock
            .Setup(h => h.Hash(command.NewPassword))
            .Returns("new-hash");

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.True(otpCode.IsUsed);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _otpCodeRepositoryMock.Verify(r => r.UpdateAsync(otpCode), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(user.Id), Times.Once);
    }
}
