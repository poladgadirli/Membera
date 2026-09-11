using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Admin.DeleteAdminAccount;

public class DeleteAdminAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<DeleteAdminAccountHandler> _logger;

    public DeleteAdminAccountHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<DeleteAdminAccountHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteAdminAccountCommand command)
    {
        if (command.TargetAdminId == command.RequestingUserId)
        {
            _logger.LogWarning("Delete admin account rejected: admin {RequestingUserId} attempted to delete their own account", command.RequestingUserId);
            throw new InvalidOperationException("Cannot perform this action on your own account.");
        }

        var user = await _userRepository.GetByIdAsync(command.TargetAdminId);
        if (user is null)
        {
            _logger.LogWarning("Delete admin account rejected: target admin {TargetAdminId} not found (requested by {RequestingUserId})", command.TargetAdminId, command.RequestingUserId);
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role != UserRole.Admin)
        {
            _logger.LogWarning("Delete admin account rejected: target user {TargetAdminId} is not an admin (requested by {RequestingUserId})", command.TargetAdminId, command.RequestingUserId);
            throw new InvalidOperationException("Target user is not an admin.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Delete admin account rejected: target admin {TargetAdminId} is already deleted (requested by {RequestingUserId})", command.TargetAdminId, command.RequestingUserId);
            throw new InvalidOperationException("User is already deleted.");
        }

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        _logger.LogInformation("Admin {RequestingUserId} deleted admin account {TargetAdminId}", command.RequestingUserId, command.TargetAdminId);
    }
}
