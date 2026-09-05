using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.ChangeEmail;

public class ChangeEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeEmailHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task HandleAsync(ChangeEmailCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        if (user.PasswordHash is null)
            throw new InvalidOperationException("This account uses Google sign-in and does not have a password. Email change via password is not available.");

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
            throw new InvalidOperationException("Current password is incorrect.");

        var existingUser = await _userRepository.GetByEmailAsync(command.NewEmail);
        if (existingUser is not null)
        {
            if (existingUser.Id == user.Id)
                return; // Yeni email cari email ilə eynidir, dəyişiklik lazım deyil.

            throw new InvalidOperationException("This email is already in use.");
        }

        user.ChangeEmail(command.NewEmail);

        await _userRepository.UpdateAsync(user);
    }
}
