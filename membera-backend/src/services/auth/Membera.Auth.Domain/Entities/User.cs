using Membera.Auth.Domain.Enums;
using Membera.Shared.Domain;

namespace Membera.Auth.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string? PasswordHash { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public AuthProvider Provider { get; private set; }
    public string? GoogleId { get; private set; }
    public UserRole Role { get; private set; }

    private User() { }

    public User(string firstName, string lastName, string email, string passwordHash, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        IsEmailVerified = false;
        IsDeleted = false;
        Provider = AuthProvider.Local;
        Role = role;
    }
    public static User CreateFromGoogle(string firstName, string lastName, string email, string googleId)
    {
        var user = new User();
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.GoogleId = googleId;
        user.Provider = AuthProvider.Google;
        user.IsEmailVerified = true; // Google has verified the email
        user.IsDeleted = false;
        user.Role = UserRole.User;
        return user;
    }

    public void MarkEmailAsVerified()
    {
        IsEmailVerified = true;
        MarkAsUpdated();
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        MarkAsUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void ChangeEmail(string newEmail)
    {
        Email = newEmail;
        IsEmailVerified = false;
        MarkAsUpdated();
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        MarkAsUpdated();
    }

    public void DemoteToUser()
    {
        Role = UserRole.User;
        MarkAsUpdated();
    }

    public string FullName => $"{FirstName} {LastName}";
}