using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;

namespace Membera.Auth.Application.Auth.Register;

public class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("Bu email artıq istifadə olunub.");

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = new User(command.FirstName, command.LastName, command.Email, passwordHash);

        await _userRepository.AddAsync(user);

        return new RegisterUserResult(user.Id, user.Email);
    }
}