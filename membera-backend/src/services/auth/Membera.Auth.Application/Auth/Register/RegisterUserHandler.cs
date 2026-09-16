using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.SendEmailVerificationOtp;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Contracts;
using Membera.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Register;

public class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventPublisher _eventPublisher;
    private readonly SendEmailVerificationOtpHandler _sendEmailVerificationOtpHandler;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEventPublisher eventPublisher,
        SendEmailVerificationOtpHandler sendEmailVerificationOtpHandler,
        ILogger<RegisterUserHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
        _sendEmailVerificationOtpHandler = sendEmailVerificationOtpHandler;
        _logger = logger;
    }

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            _logger.LogWarning("Registration attempt with already-used email: {Email}", command.Email);
            throw new InvalidOperationException("This email is already in use.");
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var role = command.IsMerchantOwner ? UserRole.MerchantOwner : UserRole.User;

        var user = new User(command.FirstName, command.LastName, command.Email, passwordHash, role);

        await _userRepository.AddAsync(user);

        _logger.LogInformation("New user registered: {UserId}, Email: {Email}, Role: {Role}", user.Id, user.Email, user.Role);

        var @event = new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            DateTime.UtcNow);

        await _eventPublisher.PublishAsync(@event, "user.registered");

        // Best-effort: a transient SMTP failure shouldn't fail the registration
        // itself, since the account was already created successfully. The user
        // can always request a new code via resend-verification.
        try
        {
            await _sendEmailVerificationOtpHandler.HandleAsync(new SendEmailVerificationOtpCommand(user.Id));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email verification OTP after registration for UserId: {UserId}", user.Id);
        }

        return new RegisterUserResult(user.Id, user.Email, user.Role.ToString());
    }
}