namespace Membera.Auth.Application.Auth.ResetPassword;

public record ResetPasswordCommand(string Email, string Code, string NewPassword);
