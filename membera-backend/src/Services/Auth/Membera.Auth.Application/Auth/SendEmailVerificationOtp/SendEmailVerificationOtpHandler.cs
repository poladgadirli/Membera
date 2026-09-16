using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Email;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.SendEmailVerificationOtp;

public class SendEmailVerificationOtpHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeRepository _otpCodeRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailVerificationOtpHandler> _logger;

    public SendEmailVerificationOtpHandler(
        IUserRepository userRepository,
        IOtpCodeRepository otpCodeRepository,
        IEmailService emailService,
        ILogger<SendEmailVerificationOtpHandler> logger)
    {
        _userRepository = userRepository;
        _otpCodeRepository = otpCodeRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(SendEmailVerificationOtpCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
        {
            _logger.LogWarning("Email verification OTP requested for unknown UserId {UserId}", command.UserId);
            throw new InvalidOperationException("User not found.");
        }

        var otpCode = new OtpCode(user.Id, OtpPurpose.EmailVerification);
        await _otpCodeRepository.AddAsync(otpCode);

        var htmlBody = BuildHtmlBody(otpCode.Code);
        await _emailService.SendEmailAsync(user.Email, "Verify your email address", htmlBody);

        _logger.LogInformation("Email verification OTP sent for UserId: {UserId}", user.Id);
    }

    private static string BuildHtmlBody(string code) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
          <h2 style="color: #111827;">Verify your email address</h2>
          <p style="color: #374151;">Use the code below to verify your email address:</p>
          <p style="font-size: 32px; font-weight: bold; letter-spacing: 4px; color: #111827;">{code}</p>
          <p style="color: #6b7280; font-size: 14px;">This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>
        </div>
        """;
}
