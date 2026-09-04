using Membera.Auth.Domain.Entities;

namespace Membera.Auth.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllByUserIdAsync(Guid userId);
}