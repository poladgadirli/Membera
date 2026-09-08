using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Admin.GetAllUsers;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Membera.Auth.Application.Tests.Auth.Admin.GetAllUsers;

public class GetAllUsersHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<GetAllUsersHandler>> _loggerMock;
    private readonly GetAllUsersHandler _handler;

    public GetAllUsersHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<GetAllUsersHandler>>();

        _handler = new GetAllUsersHandler(_userRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsNoUsers_ReturnsEmptyUserSummaryList()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _handler.HandleAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Users);
        Assert.Empty(result.Users);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsUsers_MapsEveryUserToUserSummary()
    {
        // Arrange
        var regularUser = new User("Polad", "Test", "polad@test.com", "hash-1", UserRole.User);

        var adminUser = new User("Ada", "Lovelace", "ada@test.com", "hash-2", UserRole.Admin);
        adminUser.MarkAsDeleted();

        var users = new List<User> { regularUser, adminUser };

        _userRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _handler.HandleAsync();

        // Assert
        Assert.Equal(2, result.Users.Count);

        var firstSummary = result.Users[0];
        Assert.Equal(regularUser.Id, firstSummary.Id);
        Assert.Equal(regularUser.FirstName, firstSummary.FirstName);
        Assert.Equal(regularUser.LastName, firstSummary.LastName);
        Assert.Equal(regularUser.Email, firstSummary.Email);
        Assert.Equal("User", firstSummary.Role);
        Assert.False(firstSummary.IsDeleted);
        Assert.Equal(regularUser.CreatedAt, firstSummary.CreatedAt);

        var secondSummary = result.Users[1];
        Assert.Equal(adminUser.Id, secondSummary.Id);
        Assert.Equal(adminUser.FirstName, secondSummary.FirstName);
        Assert.Equal(adminUser.LastName, secondSummary.LastName);
        Assert.Equal(adminUser.Email, secondSummary.Email);
        Assert.Equal("Admin", secondSummary.Role);
        Assert.True(secondSummary.IsDeleted);
        Assert.Equal(adminUser.CreatedAt, secondSummary.CreatedAt);
    }
}
