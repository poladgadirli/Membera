using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.ForgotPassword;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Email;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.ForgotPassword;

public class ForgotPasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOtpCodeRepository> _otpCodeRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordHandler>> _loggerMock;
    private readonly ForgotPasswordHandler _handler;

    public ForgotPasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _otpCodeRepositoryMock = new Mock<IOtpCodeRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordHandler>>();

        _handler = new ForgotPasswordHandler(
            _userRepositoryMock.Object,
            _otpCodeRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithNonexistentEmail_SilentlyNoOps()
    {
        // Arrange
        var command = new ForgotPasswordCommand("nobody@test.com");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.HandleAsync(command);

        // Assert: no exception thrown (enumeration-prevention), no OTP created, no email sent
        _otpCodeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OtpCode>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithGoogleOnlyAccount_SilentlyNoOps()
    {
        // Arrange
        var googleUser = User.CreateFromGoogle("Polad", "Test", "polad@test.com", "google-id-123");
        var command = new ForgotPasswordCommand(googleUser.Email);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(googleUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _otpCodeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OtpCode>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDeletedAccount_SilentlyNoOps()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        user.MarkAsDeleted();
        var command = new ForgotPasswordCommand(user.Email);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _otpCodeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OtpCode>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidLocalAccount_GeneratesOtpAndSendsEmail()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var command = new ForgotPasswordCommand(user.Email);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(user);

        OtpCode? capturedOtp = null;
        _otpCodeRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OtpCode>()))
            .Callback<OtpCode>(o => capturedOtp = o)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(capturedOtp);
        Assert.Equal(OtpPurpose.PasswordReset, capturedOtp!.Purpose);

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.Is<string>(body => body.Contains(capturedOtp.Code))),
            Times.Once);
    }
}
