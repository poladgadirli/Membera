using Membera.Auth.Application.Abstractions;
using Membera.Auth.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Admin.GetAllUsers;

public class GetAllUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetAllUsersHandler> _logger;

    public GetAllUsersHandler(IUserRepository userRepository, ILogger<GetAllUsersHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<GetAllUsersResult> HandleAsync(GetAllUsersQuery query)
    {
        // An unrecognized role string (shouldn't happen — the frontend only
        // sends the fixed set of role names) is treated as "no role filter"
        // rather than an error.
        UserRole? role = Enum.TryParse<UserRole>(query.Role, out var parsedRole)
            ? parsedRole
            : null;

        var (users, totalCount) = await _userRepository.GetPagedAsync(
            query.Page, query.PageSize, query.Search, role);

        var summaries = users
            .Select(user => new GetAllUsersResult.UserSummary(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role.ToString(),
                user.IsDeleted,
                user.CreatedAt))
            .ToList();

        _logger.LogInformation(
            "Admin retrieved users page {Page} (size {PageSize}). Count: {Count}, TotalCount: {TotalCount}",
            query.Page, query.PageSize, summaries.Count, totalCount);

        return new GetAllUsersResult(summaries, totalCount, query.Page, query.PageSize);
    }
}
