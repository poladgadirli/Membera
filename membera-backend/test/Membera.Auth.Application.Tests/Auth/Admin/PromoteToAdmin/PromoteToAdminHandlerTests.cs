using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Admin.PromoteToAdmin;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Admin.PromoteToAdmin;

public class PromoteToAdminHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<PromoteToAdminHandler>> _loggerMock;
    private readonly PromoteToAdminHandler _handler;

    public PromoteToAdminHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<PromoteToAdminHandler>>();

        _handler = new PromoteToAdminHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new PromoteToAdminCommand(Guid.NewGuid());

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsAlreadyAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new PromoteToAdminCommand(Guid.NewGuid());

        var adminUser = new User("Admin", "User", "admin@test.com", "hash-in-db", UserRole.Admin);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(adminUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsAlreadySuperAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new PromoteToAdminCommand(Guid.NewGuid());

        var superAdminUser = new User("Super", "Admin", "superadmin@test.com", "hash-in-db", UserRole.SuperAdmin);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(superAdminUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTargetIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new PromoteToAdminCommand(Guid.NewGuid());

        var deletedUser = new User("Polad", "Test", "polad@test.com", "hash-in-db", UserRole.User);
        deletedUser.MarkAsDeleted();

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(deletedUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command));

        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithRegularUserTarget_PromotesToAdminAndPersists()
    {
        // Arrange
        var command = new PromoteToAdminCommand(Guid.NewGuid());

        var user = new User("Polad", "Test", "polad@test.com", "hash-in-db", UserRole.User);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(command.TargetUserId))
            .ReturnsAsync(user);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(UserRole.Admin, user.Role);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}
