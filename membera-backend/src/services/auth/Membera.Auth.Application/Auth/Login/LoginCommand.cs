namespace Membera.Auth.Application.Auth.Login;

public record LoginCommand(
    string Email,
    string Password
);