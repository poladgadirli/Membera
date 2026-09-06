using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Contracts;
using Membera.Shared.Messaging;

namespace Membera.Auth.Application.Auth.Register;

public class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEventPublisher _eventPublisher;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("This email is already in use.");

        var passwordHash = _passwordHasher.Hash(command.Password);

        var role = command.IsMerchantOwner ? UserRole.MerchantOwner : UserRole.User;

        var user = new User(command.FirstName, command.LastName, command.Email, passwordHash, role);

        await _userRepository.AddAsync(user);

        var @event = new UserRegisteredEvent(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            DateTime.UtcNow);

        await _eventPublisher.PublishAsync(@event, "user.registered");

        return new RegisterUserResult(user.Id, user.Email, user.Role.ToString());
    }
}