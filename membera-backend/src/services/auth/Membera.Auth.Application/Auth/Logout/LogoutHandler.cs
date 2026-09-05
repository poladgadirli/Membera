using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.Logout;

public class LogoutHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LogoutHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task HandleAsync(LogoutCommand command)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

        if (existingToken is null)
            return;

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);
    }
}