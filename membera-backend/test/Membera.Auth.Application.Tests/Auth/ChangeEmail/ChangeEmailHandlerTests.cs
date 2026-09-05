using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.ChangeEmail;
using Membera.Auth.Domain.Entities;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.ChangeEmail;

public class ChangeEmailHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly ChangeEmailHandler _handler;

    public ChangeEmailHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _handler = new ChangeEmailHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangeEmailCommand(Guid.NewGuid(), "new@test.com", "Current123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAccountUsesGoogleSignIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangeEmailCommand(Guid.NewGuid(), "new@test.com", "Current123");

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
        var command = new ChangeEmailCommand(Guid.NewGuid(), "new@test.com", "WrongCurrent");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, user.PasswordHash!))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNewEmailBelongsToAnotherUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new ChangeEmailCommand(Guid.NewGuid(), "taken@test.com", "Current123");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");
        var otherUser = new User("Other", "User", "taken@test.com", "other-hash");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "hash-in-db"))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.NewEmail))
            .ReturnsAsync(otherUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNewEmailIsSameUsersOwnEmail_ReturnsWithoutUpdating()
    {
        // Arrange
        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");
        var command = new ChangeEmailCommand(user.Id, "polad@test.com", "Current123");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "hash-in-db"))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.NewEmail))
            .ReturnsAsync(user);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("polad@test.com", user.Email);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithAvailableNewEmail_ChangesEmailAndPersists()
    {
        // Arrange
        var command = new ChangeEmailCommand(Guid.NewGuid(), "new@test.com", "Current123");

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db");

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify(command.CurrentPassword, "hash-in-db"))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.NewEmail))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("new@test.com", user.Email);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
