using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.ChangeEmail;

public class ChangeEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ChangeEmailHandler> _logger;

    public ChangeEmailHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<ChangeEmailHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task HandleAsync(ChangeEmailCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.PasswordHash is null)
        {
            _logger.LogWarning("Email change rejected: account uses Google sign-in for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("This account uses Google sign-in and does not have a password. Email change via password is not available.");
        }

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
        {
            _logger.LogWarning("Email change rejected: current password incorrect for UserId {UserId}", command.UserId);
            throw new InvalidOperationException("Current password is incorrect.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(command.NewEmail);
        if (existingUser is not null)
        {
            if (existingUser.Id == user.Id)
                return; // Yeni email cari email ilə eynidir, dəyişiklik lazım deyil.

            _logger.LogWarning("Email change rejected: email {NewEmail} is already in use (UserId {UserId})", command.NewEmail, command.UserId);
            throw new InvalidOperationException("This email is already in use.");
        }

        var oldEmail = user.Email;

        user.ChangeEmail(command.NewEmail);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Email changed for UserId: {UserId}, OldEmail: {OldEmail}, NewEmail: {NewEmail}", user.Id, oldEmail, command.NewEmail);
    }
}
