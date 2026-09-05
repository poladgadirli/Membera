namespace Membera.Auth.Application.Auth.Login;

public record LoginResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
);