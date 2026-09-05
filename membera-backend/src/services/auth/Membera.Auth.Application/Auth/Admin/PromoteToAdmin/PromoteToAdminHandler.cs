using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Auth.Admin.PromoteToAdmin;

public class PromoteToAdminHandler
{
    private readonly IUserRepository _userRepository;

    public PromoteToAdminHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task HandleAsync(PromoteToAdminCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.Role is UserRole.Admin or UserRole.SuperAdmin)
            throw new InvalidOperationException("User is already an admin.");

        if (user.IsDeleted)
            throw new InvalidOperationException("Cannot promote a deleted user.");

        user.PromoteToAdmin();
        await _userRepository.UpdateAsync(user);
    }
}
