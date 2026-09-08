using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.DeleteAccount;

public class DeleteAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<DeleteAccountHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteAccountCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.IsDeleted)
        {
            _logger.LogWarning("Account deletion rejected: account already deleted for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("Account has already been deleted.");
        }

        if (user.PasswordHash is null)
        {
            _logger.LogWarning("Account deletion rejected: account uses Google sign-in for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("This account uses Google sign-in and does not have a password. Account deletion via password is not available.");
        }

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("Account deletion rejected: current password incorrect for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("Current password is incorrect.");
        }

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Account deleted for UserId: {UserId}", user.Id);
    }
}
