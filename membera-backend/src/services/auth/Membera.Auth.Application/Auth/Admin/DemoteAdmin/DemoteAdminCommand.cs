namespace Membera.Auth.Application.Auth.Admin.DemoteAdmin;

public record DemoteAdminCommand(
    Guid TargetUserId,
    Guid RequestingUserId
);
