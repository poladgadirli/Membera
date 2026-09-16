using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.ResetPassword;

public class ResetPasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IOtpCodeRepository otpCodeRepository,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<ResetPasswordHandler> logger)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(ResetPasswordCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning("Password reset rejected: no user found for email {Email}", command.Email);
            throw new InvalidOperationException("Invalid or expired code.");
        }

        var otpCode = await _otpCodeRepository.GetLatestAsync(user.Id, OtpPurpose.PasswordReset, command.Code);
        if (otpCode is null || !otpCode.IsValid())
        {
            _logger.LogWarning("Password reset rejected: invalid or expired code for UserId {UserId}", user.Id);
            throw new InvalidOperationException("Invalid or expired code.");
        }

        var newPasswordHash = _passwordHasher.Hash(command.NewPassword);
        user.ChangePassword(newPasswordHash);
        await _userRepository.UpdateAsync(user);

        otpCode.MarkAsUsed();
        await _otpCodeRepository.UpdateAsync(otpCode);

        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        _logger.LogInformation("Password reset completed for UserId: {UserId}", user.Id);
    }
}
