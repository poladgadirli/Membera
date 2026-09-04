using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.DeleteAccount;

public class DeleteAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DeleteAccountHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(DeleteAccountCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("İstifadəçi tapılmadı.");

        if (user.IsDeleted)
            throw new InvalidOperationException("Hesab artıq silinib.");

        var isCurrentPasswordValid = _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash);
        if (!isCurrentPasswordValid)
            throw new InvalidOperationException("Cari şifrə yanlışdır.");

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        user.MarkAsDeleted();
        await _userRepository.UpdateAsync(user);
    }
}
