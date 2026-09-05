using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.GoogleLogin;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.GoogleLogin;

public class GoogleLoginHandlerTests
{
    private readonly Mock<IGoogleAuthService> _googleAuthServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly GoogleLoginHandler _handler;

    public GoogleLoginHandlerTests()
    {
        _googleAuthServiceMock = new Mock<IGoogleAuthService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new GoogleLoginHandler(
            _googleAuthServiceMock.Object,
            _userRepositoryMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    private void SetupTokenService(User user)
    {
        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fake-access-token");

        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>()))
            .Returns(new RefreshToken("fake-refresh-token", user.Id, DateTime.UtcNow.AddDays(7)));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidGoogleToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new GoogleLoginCommand("invalid-id-token");

        _googleAuthServiceMock
            .Setup(s => s.ValidateTokenAsync(command.IdToken))
            .ReturnsAsync((GoogleUserInfo?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNewEmail_CreatesUserAndReturnsLoginResult()
    {
        // Arrange
        var command = new GoogleLoginCommand("valid-id-token");
        var googleUserInfo = new GoogleUserInfo("google-id-123", "polad@test.com", "Polad", "Test");

        _googleAuthServiceMock
            .Setup(s => s.ValidateTokenAsync(command.IdToken))
            .ReturnsAsync(googleUserInfo);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(googleUserInfo.Email))
            .ReturnsAsync((User?)null);

        User? createdUser = null;
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => createdUser = u)
            .Returns(Task.CompletedTask);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns("fake-access-token");
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken(It.IsAny<Guid>()))
            .Returns(new RefreshToken("fake-refresh-token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(googleUserInfo.Email, createdUser!.Email);
        Assert.Equal("google-id-123", createdUser.GoogleId);
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_DoesNotCreateUser()
    {
        // Arrange
        var command = new GoogleLoginCommand("valid-id-token");
        var googleUserInfo = new GoogleUserInfo("google-id-123", "polad@test.com", "Polad", "Test");

        var existingUser = new User("Polad", "Test", "polad@test.com", "some-hash", UserRole.User);

        _googleAuthServiceMock
            .Setup(s => s.ValidateTokenAsync(command.IdToken))
            .ReturnsAsync(googleUserInfo);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(googleUserInfo.Email))
            .ReturnsAsync(existingUser);

        SetupTokenService(existingUser);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDeletedUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new GoogleLoginCommand("valid-id-token");
        var googleUserInfo = new GoogleUserInfo("google-id-123", "polad@test.com", "Polad", "Test");

        var deletedUser = new User("Polad", "Test", "polad@test.com", "some-hash", UserRole.User);
        deletedUser.MarkAsDeleted();

        _googleAuthServiceMock
            .Setup(s => s.ValidateTokenAsync(command.IdToken))
            .ReturnsAsync(googleUserInfo);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(googleUserInfo.Email))
            .ReturnsAsync(deletedUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }
}
