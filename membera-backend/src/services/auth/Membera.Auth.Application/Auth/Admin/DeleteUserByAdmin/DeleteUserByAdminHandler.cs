using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Admin.DeleteUserByAdmin;

public class DeleteUserByAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<DeleteUserByAdminHandler> _logger;

    public DeleteUserByAdminHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<DeleteUserByAdminHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteUserByAdminCommand command)
    {
        if (command.TargetUserId == command.RequestingUserId)
        {
            _logger.LogWarning("Delete user by admin rejected: admin {RequestingUserId} attempted to delete their own account", command.RequestingUserId);
            throw new InvalidOperationException("Cannot perform this action on your own account.");
        }

        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
        {
            _logger.LogWarning("Delete user by admin rejected: target user {TargetUserId} not found (requested by {RequestingUserId})", command.TargetUserId, command.RequestingUserId);
            throw new InvalidOperationException("User not found.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Delete user by admin rejected: target user {TargetUserId} is already deleted (requested by {RequestingUserId})", command.TargetUserId, command.RequestingUserId);
            throw new InvalidOperationException("User is already deleted.");
        }

        if (user.Role is UserRole.Admin or UserRole.SuperAdmin)
        {
            _logger.LogWarning("Delete user by admin rejected: target user {TargetUserId} is an admin account (requested by {RequestingUserId})", command.TargetUserId, command.RequestingUserId);
            throw new InvalidOperationException("Cannot delete an admin account through this endpoint.");
        }

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        _logger.LogInformation("Admin {RequestingUserId} deleted user {TargetUserId}", command.RequestingUserId, command.TargetUserId);
    }
}
