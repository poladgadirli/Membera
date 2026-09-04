using System.Security.Claims;
using Membera.Auth.Application.Auth.ChangeEmail;
using Membera.Auth.Application.Auth.ChangePassword;
using Membera.Auth.Application.Auth.DeleteAccount;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Application.Auth.Logout;
using Membera.Auth.Application.Auth.RefreshAccessToken;
using Membera.Auth.Application.Auth.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginHandler _loginHandler;
    private readonly RefreshAccessTokenHandler _refreshAccessTokenHandler;
    private readonly LogoutHandler _logoutHandler;
    private readonly ChangePasswordHandler _changePasswordHandler;
    private readonly ChangeEmailHandler _changeEmailHandler;
    private readonly DeleteAccountHandler _deleteAccountHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginHandler loginHandler,
        RefreshAccessTokenHandler refreshAccessTokenHandler,
        LogoutHandler logoutHandler,
        ChangePasswordHandler changePasswordHandler,
        ChangeEmailHandler changeEmailHandler,
        DeleteAccountHandler deleteAccountHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginHandler = loginHandler;
        _refreshAccessTokenHandler = refreshAccessTokenHandler;
        _logoutHandler = logoutHandler;
        _changePasswordHandler = changePasswordHandler;
        _changeEmailHandler = changeEmailHandler;
        _deleteAccountHandler = deleteAccountHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var command = new RegisterUserCommand(request.FirstName, request.LastName, request.Email, request.Password);
            var result = await _registerUserHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        try
        {
            var result = await _loginHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshAccessTokenCommand command)
    {
        try
        {
            var result = await _refreshAccessTokenHandler.HandleAsync(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command)
    {
        await _logoutHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email)
                    ?? User.FindFirstValue("email");
        var firstName = User.FindFirstValue("firstName");
        var lastName = User.FindFirstValue("lastName");

        return Ok(new
        {
            userId,
            email,
            firstName,
            lastName
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        try
        {
            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
            await _changePasswordHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        try
        {
            var command = new ChangeEmailCommand(userId, request.NewEmail, request.CurrentPassword);
            await _changeEmailHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        try
        {
            var command = new DeleteAccountCommand(userId, request.CurrentPassword);
            await _deleteAccountHandler.HandleAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangeEmailRequest(string NewEmail, string CurrentPassword);

public record DeleteAccountRequest(string CurrentPassword);

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string ConfirmPassword);