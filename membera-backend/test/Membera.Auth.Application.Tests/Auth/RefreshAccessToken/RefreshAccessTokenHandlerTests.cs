using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.RefreshAccessToken;
using Membera.Auth.Domain.Entities;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.RefreshAccessToken;

public class RefreshAccessTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshAccessTokenHandler _handler;

    public RefreshAccessTokenHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _handler = new RefreshAccessTokenHandler(
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownRefreshToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RefreshAccessTokenCommand("does-not-exist");

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithRevokedRefreshToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RefreshAccessTokenCommand("revoked-token");

        var revokedToken = new RefreshToken(command.RefreshToken, Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        revokedToken.Revoke();

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync(revokedToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExpiredRefreshToken_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RefreshAccessTokenCommand("expired-token");

        var expiredToken = new RefreshToken(command.RefreshToken, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync(expiredToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RefreshAccessTokenCommand("active-token");

        var userId = Guid.NewGuid();
        var activeToken = new RefreshToken(command.RefreshToken, userId, DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync(activeToken);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithActiveRefreshToken_RevokesOldTokenAndReturnsNewTokens()
    {
        // Arrange
        var command = new RefreshAccessTokenCommand("active-token");

        var user = new User("Polad", "Test", "polad@test.com", "some-hash");
        var oldToken = new RefreshToken(command.RefreshToken, user.Id, DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync(oldToken);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id))
            .ReturnsAsync(user);

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(user))
            .Returns("new-access-token");

        var newRefreshToken = new RefreshToken("new-refresh-token", user.Id, DateTime.UtcNow.AddDays(7));
        _tokenServiceMock
            .Setup(t => t.GenerateRefreshToken(user.Id))
            .Returns(newRefreshToken);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.True(oldToken.IsRevoked);
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(oldToken), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(newRefreshToken), Times.Once);
    }
}
