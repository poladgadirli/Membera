namespace Membera.Auth.Application.Auth.Admin.PromoteToAdmin;

public record PromoteToAdminCommand(
    Guid TargetUserId
);
