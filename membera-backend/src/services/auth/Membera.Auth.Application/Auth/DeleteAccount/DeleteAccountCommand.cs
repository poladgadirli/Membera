namespace Membera.Auth.Application.Auth.DeleteAccount;

public record DeleteAccountCommand(
    Guid UserId,
    string CurrentPassword
);
