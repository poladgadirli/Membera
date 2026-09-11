using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.RefreshAccessToken;

public class RefreshAccessTokenHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<RefreshAccessTokenHandler> _logger;

    public RefreshAccessTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<RefreshAccessTokenHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<RefreshAccessTokenResult> HandleAsync(RefreshAccessTokenCommand command)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            _logger.LogWarning("Refresh token rejected: token is missing, expired, or revoked.");
            throw new InvalidOperationException("Invalid refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId);
        if (user is null)
        {
            _logger.LogWarning("Refresh token rejected: no user found for UserId {UserId}", existingToken.UserId);
            throw new InvalidOperationException("User not found.");
        }

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        _logger.LogInformation("Access token refreshed for UserId: {UserId}", user.Id);

        return new RefreshAccessTokenResult(newAccessToken, newRefreshToken.Token);
    }
}
