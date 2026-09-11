using Google.Apis.Auth;
using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Infrastructure.Security;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateTokenAsync(string idToken)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleUserInfo(
                GoogleId: payload.Subject,
                Email: payload.Email,
                FirstName: payload.GivenName ?? string.Empty,
                LastName: payload.FamilyName ?? string.Empty
            );
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google ID token.");
            return null;
        }
    }
}