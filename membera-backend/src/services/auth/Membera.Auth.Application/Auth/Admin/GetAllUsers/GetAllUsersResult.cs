namespace Membera.Auth.Application.Auth.Admin.GetAllUsers;

public record GetAllUsersResult(List<GetAllUsersResult.UserSummary> Users, int TotalCount, int Page, int PageSize)
{
    public record UserSummary(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Role,
        bool IsDeleted,
        DateTime CreatedAt
    );
}
