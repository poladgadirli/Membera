namespace Membera.Auth.Application.Abstractions;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateTokenAsync(string idToken);
}

public record GoogleUserInfo(
    string GoogleId,
    string Email,
    string FirstName,
    string LastName
);