using System.Security.Cryptography;
using Membera.Auth.Domain.Enums;
using Membera.Shared.Domain;

namespace Membera.Auth.Domain.Entities;

public class OtpCode : BaseEntity
{
    private const int ExpirationMinutes = 10;

    public Guid UserId { get; private set; }
    public string Code { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }

    private OtpCode() { }

    public OtpCode(Guid userId, OtpPurpose purpose)
    {
        UserId = userId;
        Purpose = purpose;
        Code = GenerateCode();
        ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
        IsUsed = false;
    }

    public void MarkAsUsed()
    {
        IsUsed = true;
        MarkAsUpdated();
    }

    public bool IsValid() => !IsUsed && DateTime.UtcNow < ExpiresAt;

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
