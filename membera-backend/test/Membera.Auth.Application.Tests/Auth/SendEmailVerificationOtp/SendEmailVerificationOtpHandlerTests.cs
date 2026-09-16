using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.SendEmailVerificationOtp;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Email;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.SendEmailVerificationOtp;

public class SendEmailVerificationOtpHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IOtpCodeRepository> _otpCodeRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<SendEmailVerificationOtpHandler>> _loggerMock;
    private readonly SendEmailVerificationOtpHandler _handler;

    public SendEmailVerificationOtpHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _otpCodeRepositoryMock = new Mock<IOtpCodeRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<SendEmailVerificationOtpHandler>>();

        _handler = new SendEmailVerificationOtpHandler(
            _userRepositoryMock.Object,
            _otpCodeRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new SendEmailVerificationOtpCommand(Guid.NewGuid());

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _otpCodeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<OtpCode>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidUser_GeneratesOtpAndSendsEmail()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash", UserRole.User);
        var command = new SendEmailVerificationOtpCommand(user.Id);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
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
        Assert.Equal(OtpPurpose.EmailVerification, capturedOtp!.Purpose);
        Assert.Matches("^[0-9]{6}$", capturedOtp.Code);

        _emailServiceMock.Verify(
            e => e.SendEmailAsync(user.Email, It.IsAny<string>(), It.Is<string>(body => body.Contains(capturedOtp.Code))),
            Times.Once);
    }
}
