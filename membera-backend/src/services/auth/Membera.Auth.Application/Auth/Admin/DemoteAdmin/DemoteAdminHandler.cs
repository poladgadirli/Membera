using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Auth.Admin.DemoteAdmin;

public class DemoteAdminHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DemoteAdminHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(DemoteAdminCommand command)
    {
        if (command.TargetUserId == command.RequestingUserId)
            throw new InvalidOperationException("Cannot perform this action on your own account.");

        var user = await _userRepository.GetByIdAsync(command.TargetUserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.Role != UserRole.Admin)
            throw new InvalidOperationException("User is not an admin.");

        user.DemoteToUser();
        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);
    }
}
