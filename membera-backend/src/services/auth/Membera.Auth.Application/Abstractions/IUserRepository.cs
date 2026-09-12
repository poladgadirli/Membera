using Membera.Auth.Domain.Entities;
using Membera.Auth.Domain.Enums;

namespace Membera.Auth.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>One page of users, optionally filtered by a case-insensitive
    /// substring match against first name / last name / email, and/or an
    /// exact role match. TotalCount reflects the filtered result set.</summary>
    Task<(List<User> Users, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, UserRole? role = null);

    Task AddAsync(User user);
    Task UpdateAsync(User user);
}