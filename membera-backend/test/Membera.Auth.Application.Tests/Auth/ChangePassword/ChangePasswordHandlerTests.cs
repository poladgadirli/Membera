using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.ChangePassword;
using Membera.Auth.Domain.Entities;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.ChangePassword;

public class ChangePasswordHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly ChangePasswordHandler _handler;

    public ChangePasswordHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new ChangePasswordHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "Current123", "NewPass123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountUsesGoogleSignIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "Current123", "NewPass123");

        var googleUser = User.CreateFromGoogle("Polad", "Test", "polad@test.com", "google-id-123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(googleUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _passwordHasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithWrongCurrentPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "WrongCurrent", "NewPass123");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, user.PasswordHash!))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidCurrentPassword_ChangesPasswordAndRevokesAllRefreshTokens()
    {
        // Arrange
        var command = new ChangePasswordCommand(Guid.NewGuid(), "Current123", "NewPass123");

        var user = new User("Polad", "Test", "polad@test.com", "old-hash");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "old-hash"))
            .Returns(true);

        _passwordHasherMock
            .Setup(h => h.Hash(command.NewPassword))
            .Returns("new-hash");

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("new-hash", user.PasswordHash);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(user.Id), Times.Once);
    }
}
