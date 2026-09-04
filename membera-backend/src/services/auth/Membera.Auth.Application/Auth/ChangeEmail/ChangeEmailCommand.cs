namespace Membera.Auth.Application.Auth.ChangeEmail;

public record ChangeEmailCommand(
    Guid UserId,
    string NewEmail,
    string CurrentPassword
);
