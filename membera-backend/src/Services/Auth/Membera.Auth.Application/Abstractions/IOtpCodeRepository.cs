using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Abstractions;

public interface IOtpCodeRepository
{
    Task AddAsync(OtpCode otpCode);

    /// <summary>The most recently created OtpCode matching the given user, purpose
    /// and code, regardless of whether it is still valid. Callers should check
    /// IsValid() on the result.</summary>
    Task<OtpCode?> GetLatestAsync(Guid userId, OtpPurpose purpose, string code);

    Task UpdateAsync(OtpCode otpCode);
}
