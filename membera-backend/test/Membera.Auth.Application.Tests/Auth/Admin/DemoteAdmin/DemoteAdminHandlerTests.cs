using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Admin.DemoteAdmin;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Admin.DemoteAdmin;

public class DemoteAdminHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly DemoteAdminHandler _handler;

    public DemoteAdminHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new DemoteAdminHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsRequestingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var sharedId = Guid.NewGuid();
        var command = new DemoteAdminCommand(sharedId, sharedId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DemoteAdminCommand(Guid.NewGuid(), Guid.NewGuid());

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsNotAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DemoteAdminCommand(Guid.NewGuid(), Guid.NewGuid());

        var regularUser = new User("Polad", "Test", "polad@test.com", "hash-in-db", UserRole.User);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(regularUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsSuperAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new DemoteAdminCommand(Guid.NewGuid(), Guid.NewGuid());

        var superAdminUser = new User("Super", "Admin", "superadmin@test.com", "hash-in-db", UserRole.SuperAdmin);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(superAdminUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithAdminTarget_DemotesToUserAndRevokesAllRefreshTokens()
    {
        // Arrange
        var command = new DemoteAdminCommand(Guid.NewGuid(), Guid.NewGuid());

        var adminUser = new User("Admin", "User", "admin@test.com", "hash-in-db", UserRole.Admin);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(adminUser);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(UserRole.User, adminUser.Role);
        _userRepositoryMock.Verify(r => r.UpdateAsync(adminUser), Times.Once);
        _refreshTokenRepositoryMock.Verify(r => r.RevokeAllByUserIdAsync(adminUser.Id), Times.Once);
    }
}
