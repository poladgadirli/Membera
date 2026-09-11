namespace Membera.Auth.Application.Auth.Admin.DeleteUserByAdmin;

public record DeleteUserByAdminCommand(
    Guid TargetUserId,
    Guid RequestingUserId
);
