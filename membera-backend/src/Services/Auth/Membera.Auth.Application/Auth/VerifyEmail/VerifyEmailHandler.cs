using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.VerifyEmail;

public class VerifyEmailHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly ILogger<VerifyEmailHandler> _logger;

    public VerifyEmailHandler(
        IUserRepository userRepository,
        IOtpCodeRepository otpCodeRepository,
        ILogger<VerifyEmailHandler> logger)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _logger = logger;
    }

    public async Task HandleAsync(VerifyEmailCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new InvalidOperationException("User not found.");

        var otpCode = await _otpCodeRepository.GetLatestAsync(user.Id, OtpPurpose.EmailVerification, command.Code);
        if (otpCode is null || !otpCode.IsValid())
        {
            _logger.LogWarning("Email verification rejected: invalid or expired code for UserId {UserId}", user.Id);
            throw new InvalidOperationException("Invalid or expired code.");
        }

        otpCode.MarkAsUsed();
        await _otpCodeRepository.UpdateAsync(otpCode);

        user.MarkEmailAsVerified();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Email verified for UserId: {UserId}", user.Id);
    }
}
