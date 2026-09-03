using Membera.Auth.Domain.Entities;

namespace Membera.Auth.Application.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
}