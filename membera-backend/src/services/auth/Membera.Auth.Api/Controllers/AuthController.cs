using Membera.Auth.Application.Auth.ChangeEmail;
using Membera.Auth.Application.Auth.ChangePassword;
using Membera.Auth.Application.Auth.DeleteAccount;
using Membera.Auth.Application.Auth.ForgotPassword;
using Membera.Auth.Application.Auth.GoogleLogin;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Application.Auth.Logout;
using Membera.Auth.Application.Auth.RefreshAccessToken;
using Membera.Auth.Application.Auth.Register;
using Membera.Auth.Application.Auth.ResetPassword;
using Membera.Auth.Application.Auth.SendEmailVerificationOtp;
using Membera.Auth.Application.Auth.VerifyEmail;
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
    private readonly VerifyEmailHandler _verifyEmailHandler;
    private readonly SendEmailVerificationOtpHandler _sendEmailVerificationOtpHandler;
    private readonly ForgotPasswordHandler _forgotPasswordHandler;
    private readonly ResetPasswordHandler _resetPasswordHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginHandler loginHandler,
        RefreshAccessTokenHandler refreshAccessTokenHandler,
        LogoutHandler logoutHandler,
        ChangePasswordHandler changePasswordHandler,
        ChangeEmailHandler changeEmailHandler,
        DeleteAccountHandler deleteAccountHandler,
        GoogleLoginHandler googleLoginHandler,
        VerifyEmailHandler verifyEmailHandler,
        SendEmailVerificationOtpHandler sendEmailVerificationOtpHandler,
        ForgotPasswordHandler forgotPasswordHandler,
        ResetPasswordHandler resetPasswordHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginHandler = loginHandler;
        _refreshAccessTokenHandler = refreshAccessTokenHandler;
        _logoutHandler = logoutHandler;
        _changePasswordHandler = changePasswordHandler;
        _changeEmailHandler = changeEmailHandler;
        _deleteAccountHandler = deleteAccountHandler;
        _googleLoginHandler = googleLoginHandler;
        _verifyEmailHandler = verifyEmailHandler;
        _sendEmailVerificationOtpHandler = sendEmailVerificationOtpHandler;
        _forgotPasswordHandler = forgotPasswordHandler;
        _resetPasswordHandler = resetPasswordHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var command = new RegisterUserCommand(request.FirstName, request.LastName, request.Email, request.Password, request.IsMerchantOwner);
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
        var emailVerified = User.FindFirstValue("emailVerified");
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(BaseResponse<object>.SuccessResponse(new
        {
            userId,
            email,
            firstName,
            lastName,
            emailVerified,
            role
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

    [Authorize]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        var command = new VerifyEmailCommand(userId, request.Code);
        await _verifyEmailHandler.HandleAsync(command);
        return NoContent();
    }

    [Authorize]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                                ?? User.FindFirstValue("sub")!);

        var command = new SendEmailVerificationOtpCommand(userId);
        await _sendEmailVerificationOtpHandler.HandleAsync(command);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await _forgotPasswordHandler.HandleAsync(command);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Code, request.NewPassword);
        await _resetPasswordHandler.HandleAsync(command);
        return NoContent();
    }
}

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record ChangeEmailRequest(string NewEmail, string CurrentPassword);

public record DeleteAccountRequest(string CurrentPassword);

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, bool IsMerchantOwner);

public record VerifyEmailRequest(string Code);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Code, string NewPassword, string ConfirmNewPassword);