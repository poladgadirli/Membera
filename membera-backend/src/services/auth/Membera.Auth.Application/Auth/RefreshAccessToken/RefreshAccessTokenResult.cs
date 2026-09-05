namespace Membera.Auth.Application.Auth.RefreshAccessToken;

public record RefreshAccessTokenResult(
    string AccessToken,
    string RefreshToken
);