namespace Membera.Shared.Contracts;

public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime RegisteredAt
);