using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Admin.PromoteToAdmin;

public class PromoteToAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PromoteToAdminHandler> _logger;

    public PromoteToAdminHandler(IUserRepository userRepository, ILogger<PromoteToAdminHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task HandleAsync(PromoteToAdminCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
        {
            _logger.LogWarning("Promote to admin rejected: target user {TargetUserId} not found", command.TargetUserId);
            throw new InvalidOperationException("User not found.");
        }

        if (user.Role is UserRole.Admin or UserRole.SuperAdmin)
        {
            _logger.LogWarning("Promote to admin rejected: target user {TargetUserId} is already an admin", command.TargetUserId);
            throw new InvalidOperationException("User is already an admin.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Promote to admin rejected: target user {TargetUserId} is deleted", command.TargetUserId);
            throw new InvalidOperationException("Cannot promote a deleted user.");
        }

        user.PromoteToAdmin();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("User {TargetUserId} promoted to admin", command.TargetUserId);
    }
}
