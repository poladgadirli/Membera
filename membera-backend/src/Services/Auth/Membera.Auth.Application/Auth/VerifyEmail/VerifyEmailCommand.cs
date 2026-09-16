namespace Membera.Auth.Application.Auth.VerifyEmail;

public record VerifyEmailCommand(Guid UserId, string Code);
