using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Login;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new LoginHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithWrongPassword_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new LoginCommand("polad@test.com", "WrongPassword");

        var existingUser = new User("Polad", "Test", "polad@test.com", "correct-hash-in-db", UserRole.User);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, existingUser.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new LoginCommand("notfound@test.com", "AnyPassword");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithDeletedAccount_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new LoginCommand("polad@test.com", "CorrectPassword");

        var deletedUser = new User("Polad", "Test", "polad@test.com", "some-hash", UserRole.User);
        deletedUser.MarkAsDeleted();

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(deletedUser);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, deletedUser.PasswordHash))
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsLoginResult()
    {
        // Arrange
        var command = new LoginCommand("polad@test.com", "CorrectPassword");

        var existingUser = new User("Polad", "Test", "polad@test.com", "correct-hash", UserRole.User);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        _passwordHasherMock
            .Setup(h => h.Verify(command.Password, existingUser.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(existingUser))
            .Returns("fake-access-token");

        var fakeRefreshToken = new RefreshToken("fake-refresh-token", existingUser.Id, DateTime.UtcNow.AddDays(7));
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken(existingUser.Id))
            .Returns(fakeRefreshToken);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(fakeRefreshToken), Times.Once);
    }
}