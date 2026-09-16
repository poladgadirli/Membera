using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Email;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.ForgotPassword;

public class ForgotPasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordHandler> _logger;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IOtpCodeRepository otpCodeRepository,
        IEmailService emailService,
        ILogger<ForgotPasswordHandler> logger)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(ForgotPasswordCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);

        // Enumeration prevention: a nonexistent email, a deleted account, and a
        // Google-only account (no password to reset) all silently no-op instead
        // of surfacing an error, so callers can't use this endpoint to discover
        // which emails are registered.
        if (user is null || user.IsDeleted || user.PasswordHash is null)
        {
            _logger.LogWarning("Password reset requested for non-resettable account: {Email}", command.Email);
            return;
        }

        var otpCode = new OtpCode(user.Id, OtpPurpose.PasswordReset);
        await _otpCodeRepository.AddAsync(otpCode);

        var htmlBody = BuildHtmlBody(otpCode.Code);
        await _emailService.SendEmailAsync(user.Email, "Reset your password", htmlBody);

        _logger.LogInformation("Password reset OTP sent for UserId: {UserId}", user.Id);
    }

    private static string BuildHtmlBody(string code) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
          <h2 style="color: #111827;">Reset your password</h2>
          <p style="color: #374151;">Use the code below to reset your password:</p>
          <p style="font-size: 32px; font-weight: bold; letter-spacing: 4px; color: #111827;">{code}</p>
          <p style="color: #6b7280; font-size: 14px;">This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>
        </div>
        """;
}
