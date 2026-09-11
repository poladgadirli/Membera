using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Admin.DemoteAdmin;

public class DemoteAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<DemoteAdminHandler> _logger;

    public DemoteAdminHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<DemoteAdminHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(DemoteAdminCommand command)
    {
        if (command.TargetUserId == command.RequestingUserId)
        {
            _logger.LogWarning("Demote admin rejected: admin {RequestingUserId} attempted to demote their own account", command.RequestingUserId);
            throw new InvalidOperationException("Cannot perform this action on your own account.");
        }

        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
        {
            _logger.LogWarning("Demote admin rejected: target user {TargetUserId} not found (requested by {RequestingUserId})", command.TargetUserId, command.RequestingUserId);
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role != UserRole.Admin)
        {
            _logger.LogWarning("Demote admin rejected: target user {TargetUserId} is not an admin (requested by {RequestingUserId})", command.TargetUserId, command.RequestingUserId);
            throw new InvalidOperationException("User is not an admin.");
        }

        user.DemoteToUser();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        _logger.LogInformation("Admin {RequestingUserId} demoted admin {TargetUserId} to user", command.RequestingUserId, command.TargetUserId);
    }
}
