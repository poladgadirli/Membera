namespace Membera.Auth.Application.Auth.Admin.DeleteAdminAccount;

public record DeleteAdminAccountCommand(
    Guid TargetAdminId,
    Guid RequestingUserId
);
