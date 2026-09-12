using Membera.Auth.Application.Auth.Admin.DeleteAdminAccount;
using Membera.Auth.Application.Auth.Admin.DeleteUserByAdmin;
using Membera.Auth.Application.Auth.Admin.DemoteAdmin;
using Membera.Auth.Application.Auth.Admin.GetAllUsers;
using Membera.Auth.Application.Auth.Admin.PromoteToAdmin;
using Membera.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Membera.Auth.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly GetAllUsersHandler _getAllUsersHandler;
    private readonly DeleteUserByAdminHandler _deleteUserByAdminHandler;
    private readonly PromoteToAdminHandler _promoteToAdminHandler;
    private readonly DemoteAdminHandler _demoteAdminHandler;
    private readonly DeleteAdminAccountHandler _deleteAdminAccountHandler;

    public AdminController(
        GetAllUsersHandler getAllUsersHandler,
        DeleteUserByAdminHandler deleteUserByAdminHandler,
        PromoteToAdminHandler promoteToAdminHandler,
        DemoteAdminHandler demoteAdminHandler,
        DeleteAdminAccountHandler deleteAdminAccountHandler)
    {
        _getAllUsersHandler = getAllUsersHandler;
        _deleteUserByAdminHandler = deleteUserByAdminHandler;
        _promoteToAdminHandler = promoteToAdminHandler;
        _demoteAdminHandler = demoteAdminHandler;
        _deleteAdminAccountHandler = deleteAdminAccountHandler;
    }

    private Guid GetRequestingUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue("sub")!);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        int page = 1, int pageSize = 10, string? search = null, string? role = null)
    {
        var result = await _getAllUsersHandler.HandleAsync(
            new GetAllUsersQuery(page, pageSize, search, role));
        return Ok(BaseResponse<GetAllUsersResult>.SuccessResponse(result));
    }

    [HttpDelete("users/{targetUserId}")]
    public async Task<IActionResult> DeleteUser(Guid targetUserId)
    {
        var command = new DeleteUserByAdminCommand(targetUserId, GetRequestingUserId());
        await _deleteUserByAdminHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("promote/{targetUserId}")]
    public async Task<IActionResult> PromoteToAdmin(Guid targetUserId)
    {
        var command = new PromoteToAdminCommand(targetUserId);
        await _promoteToAdminHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("demote/{targetUserId}")]
    public async Task<IActionResult> DemoteAdmin(Guid targetUserId)
    {
        var command = new DemoteAdminCommand(targetUserId, GetRequestingUserId());
        await _demoteAdminHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpDelete("{targetAdminId}")]
    public async Task<IActionResult> DeleteAdminAccount(Guid targetAdminId)
    {
        var command = new DeleteAdminAccountCommand(targetAdminId, GetRequestingUserId());
        await _deleteAdminAccountHandler.HandleAsync(command);
        return NoContent();
    }
}
