using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Membera.Auth.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        // Read the signing key the same way Program.cs reads the validation key:
        // environment variable (.env / OS) first, falling back to configuration.
        // Keeping both sides on an identical source/priority prevents
        // "Signature validation failed" from key drift between signing and validation.
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSection["SecretKey"]!;
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expirationMinutes = int.Parse(jwtSection["AccessTokenExpirationMinutes"]!);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var expirationDays = int.Parse(jwtSection["RefreshTokenExpirationDays"]!);

        var randomBytes = new byte[64];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var token = Convert.ToBase64String(randomBytes);

        return new RefreshToken(token, userId, DateTime.UtcNow.AddDays(expirationDays));
    }
}