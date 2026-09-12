namespace Membera.Auth.Application.Auth.Admin.GetAllUsers;

public record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Role = null);
