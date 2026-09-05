using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.Admin.GetAllUsers;

public class GetAllUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetAllUsersResult> HandleAsync()
    {
        var users = await _userRepository.GetAllAsync();

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

        return new GetAllUsersResult(summaries);
    }
}
