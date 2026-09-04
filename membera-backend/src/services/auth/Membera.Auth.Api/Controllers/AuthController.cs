using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Application.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginHandler _loginHandler;

    public AuthController(RegisterUserHandler registerUserHandler, LoginHandler loginHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginHandler = loginHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand command)
    {
        try
        {
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
}