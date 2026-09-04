using Membera.Auth.Application.Auth.Register;
using Microsoft.AspNetCore.Mvc;

namespace Membera.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;

    public AuthController(RegisterUserHandler registerUserHandler)
    {
        _registerUserHandler = registerUserHandler;
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
}