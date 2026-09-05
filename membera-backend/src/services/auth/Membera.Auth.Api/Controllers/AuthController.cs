using Membera.Auth.Application.Auth.ChangeEmail;
using Membera.Auth.Application.Auth.ChangePassword;
using Membera.Auth.Application.Auth.DeleteAccount;
using Membera.Auth.Application.Auth.GoogleLogin;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Application.Auth.Logout;
using Membera.Auth.Application.Auth.RefreshAccessToken;
using Membera.Auth.Application.Auth.Register;
using Membera.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    private readonly GoogleLoginHandler _googleLoginHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginHandler loginHandler,
        RefreshAccessTokenHandler refreshAccessTokenHandler,
        LogoutHandler logoutHandler,
        ChangePasswordHandler changePasswordHandler,
        ChangeEmailHandler changeEmailHandler,
        DeleteAccountHandler deleteAccountHandler,
        GoogleLoginHandler googleLoginHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginHandler = loginHandler;
        _refreshAccessTokenHandler = refreshAccessTokenHandler;
        _logoutHandler = logoutHandler;
        _changePasswordHandler = changePasswordHandler;
        _changeEmailHandler = changeEmailHandler;
        _deleteAccountHandler = deleteAccountHandler;
        _googleLoginHandler = googleLoginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterUserCommand(request.FirstName, request.LastName, request.Email, request.Password);
        var result = await _registerUserHandler.HandleAsync(command);
        return Ok(BaseResponse<RegisterUserResult>.SuccessResponse(result));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        try
        {
            var result = await _loginHandler.HandleAsync(command);
            return Ok(BaseResponse<LoginResult>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(BaseResponse<object>.FailureResponse(ex.Message));
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshAccessTokenCommand command)
    {
        try
        {
            var result = await _refreshAccessTokenHandler.HandleAsync(command);
            return Ok(BaseResponse<RefreshAccessTokenResult>.SuccessResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(BaseResponse<object>.FailureResponse(ex.Message));
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

        return Ok(BaseResponse<object>.SuccessResponse(new
        {
            userId,
            email,
            firstName,
            lastName
        }));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);
        await _changePasswordHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        var command = new ChangeEmailCommand(userId, request.NewEmail, request.CurrentPassword);
        await _changeEmailHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        var command = new DeleteAccountCommand(userId, request.CurrentPassword);
        await _deleteAccountHandler.HandleAsync(command);
        return NoContent();
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginCommand command)
    {
        var result = await _googleLoginHandler.HandleAsync(command);
        return Ok(BaseResponse<Membera.Auth.Application.Auth.Login.LoginResult>.SuccessResponse(result));
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangeEmailRequest(string NewEmail, string CurrentPassword);

public record DeleteAccountRequest(string CurrentPassword);

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string ConfirmPassword);