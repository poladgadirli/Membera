using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.ChangePassword;

public class ChangePasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<ChangePasswordHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ChangePasswordCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.PasswordHash is null)
        {
            _logger.LogWarning("Password change rejected: account uses Google sign-in for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("This account uses Google sign-in and does not have a password. Password change is not available.");
        }

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("Password change rejected: current password incorrect for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("Current password is incorrect.");
        }

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        _logger.LogInformation("Password changed for UserId: {UserId}", user.Id);
    }
}
