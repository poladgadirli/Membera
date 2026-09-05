using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Register;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Register;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new RegisterUserHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithNewEmail_CreatesUserSuccessfully()
    {
        // Arrange
        var command = new RegisterUserCommand("Polad", "Test", "polad@test.com", "Password123", false);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.Hash(command.Password))
            .Returns("hashed-password-123");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(command.Email, result.Email);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterUserCommand("Polad", "Test", "polad@test.com", "Password123", false);

        var existingUser = new User("Existing", "User", command.Email, "some-hash", UserRole.User);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNewEmail_HashesPasswordBeforeSaving()
    {
        // Arrange
        var command = new RegisterUserCommand("Polad", "Test", "polad@test.com", "PlainPassword123", false);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(h => h.Hash(command.Password))
            .Returns("hashed-version");

        User? capturedUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("hashed-version", capturedUser!.PasswordHash);
        Assert.NotEqual(command.Password, capturedUser.PasswordHash);
    }
}