using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Logout;
using Membera.Auth.Domain.Entities;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Logout;

public class LogoutHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly LogoutHandler _handler;

    public LogoutHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _handler = new LogoutHandler(_refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentRefreshToken_ReturnsSilentlyWithoutRevoking()
    {
        // Arrange
        var command = new LogoutCommand("does-not-exist");

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithExistingRefreshToken_RevokesAndPersistsIt()
    {
        // Arrange
        var command = new LogoutCommand("existing-token");

        var existingToken = new RefreshToken(command.RefreshToken, Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync(existingToken);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.True(existingToken.IsRevoked);
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(existingToken), Times.Once);
    }
}
