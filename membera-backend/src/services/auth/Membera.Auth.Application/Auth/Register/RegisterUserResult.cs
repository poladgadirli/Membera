namespace Membera.Auth.Application.Auth.Register;

public record RegisterUserResult(
    Guid UserId,
    string Email
);