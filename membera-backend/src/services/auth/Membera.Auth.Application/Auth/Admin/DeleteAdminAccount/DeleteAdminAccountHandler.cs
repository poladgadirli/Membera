using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Auth.Admin.DeleteAdminAccount;

public class DeleteAdminAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DeleteAdminAccountHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(DeleteAdminAccountCommand command)
    {
        if (command.TargetAdminId == command.RequestingUserId)
            throw new InvalidOperationException("Cannot perform this action on your own account.");

        var user = await _userRepository.GetByIdAsync(command.TargetAdminId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.Role != UserRole.Admin)
            throw new InvalidOperationException("Target user is not an admin.");

        if (user.IsDeleted)
            throw new InvalidOperationException("User is already deleted.");

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);
    }
}
