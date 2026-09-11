using Membera.Auth.Application.Abstractions;
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
        var (users, totalCount) = await _userRepository.GetPagedAsync(query.Page, query.PageSize);

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
