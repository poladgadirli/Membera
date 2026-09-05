using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Auth.Admin.DeleteUserByAdmin;

public class DeleteUserByAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DeleteUserByAdminHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(DeleteUserByAdminCommand command)
    {
        if (command.TargetUserId == command.RequestingUserId)
            throw new InvalidOperationException("Cannot perform this action on your own account.");

        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.IsDeleted)
            throw new InvalidOperationException("User is already deleted.");

        if (user.Role is UserRole.Admin or UserRole.SuperAdmin)
            throw new InvalidOperationException("Cannot delete an admin account through this endpoint.");

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);
    }
}
