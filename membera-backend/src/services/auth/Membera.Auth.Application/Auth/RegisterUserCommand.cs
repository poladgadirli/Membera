namespace Membera.Auth.Application.Auth.Register;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password
);