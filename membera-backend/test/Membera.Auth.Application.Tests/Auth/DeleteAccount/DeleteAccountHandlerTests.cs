using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.DeleteAccount;
using Membera.Auth.Domain.Entities;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.DeleteAccount;

public class DeleteAccountHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly DeleteAccountHandler _handler;

    public DeleteAccountHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new DeleteAccountHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid(), "Current123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountAlreadyDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid(), "Current123");

        var deletedUser = new User("Polad", "Test", "polad@test.com", "hash-in-db");
        deletedUser.MarkAsDeleted();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(deletedUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _passwordHasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountUsesGoogleSignIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid(), "Current123");

        var googleUser = User.CreateFromGoogle("Polad", "Test", "polad@test.com", "google-id-123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(googleUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _passwordHasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithWrongCurrentPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid(), "WrongCurrent");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "hash-in-db"))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithValidCurrentPassword_MarksAccountDeletedAndRevokesAllRefreshTokens()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid(), "Current123");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "hash-in-db"))
            .Returns(true);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.True(user.IsDeleted);
        Assert.NotNull(user.DeletedAt);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(user.Id), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
