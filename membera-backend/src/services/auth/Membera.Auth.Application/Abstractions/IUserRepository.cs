using Membera.Auth.Domain.Entities;

namespace Membera.Auth.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<(List<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}