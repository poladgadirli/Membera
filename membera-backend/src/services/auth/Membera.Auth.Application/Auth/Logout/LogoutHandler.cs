using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Logout;

public class LogoutHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LogoutHandler> _logger;

    public LogoutHandler(IRefreshTokenRepository refreshTokenRepository, ILogger<LogoutHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task HandleAsync(LogoutCommand command)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

        if (existingToken is null)
            return;

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);

        _logger.LogInformation("Refresh token revoked on logout for UserId: {UserId}", existingToken.UserId);
    }
}
