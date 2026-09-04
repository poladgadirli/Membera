using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.ChangePassword;

public class ChangePasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(ChangePasswordCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
            throw new InvalidOperationException("Current password is incorrect.");

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);
    }
}