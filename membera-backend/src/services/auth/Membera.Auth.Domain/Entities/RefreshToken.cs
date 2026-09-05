using Membera.Shared.Domain;

namespace Membera.Auth.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public RefreshToken(string token, Guid userId, DateTime expiresAt)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
        MarkAsUpdated();
    }
}