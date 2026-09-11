using System.Text.Json;
using Membera.Shared.Contracts;

namespace Membera.Auth.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Biznes qaydası pozuntusu: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = JsonSerializer.Serialize(BaseResponse<object>.FailureResponse(ex.Message));
            await context.Response.WriteAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gözlənilməz xəta baş verdi.");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response = JsonSerializer.Serialize(BaseResponse<object>.FailureResponse("An internal server error occurred."));
            await context.Response.WriteAsync(response);
        }
    }
}