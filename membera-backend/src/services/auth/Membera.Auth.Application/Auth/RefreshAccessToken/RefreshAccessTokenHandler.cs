using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.RefreshAccessToken;

public class RefreshAccessTokenHandler
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshAccessTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<RefreshAccessTokenResult> HandleAsync(RefreshAccessTokenCommand command)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(command.RefreshToken);

        if (existingToken is null || !existingToken.IsActive)
            throw new InvalidOperationException("Refresh token etibarsızdır.");

        var user = await _userRepository.GetByIdAsync(existingToken.UserId);
        if (user is null)
            throw new InvalidOperationException("İstifadəçi tapılmadı.");

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _refreshTokenRepository.AddAsync(newRefreshToken);

        return new RefreshAccessTokenResult(newAccessToken, newRefreshToken.Token);
    }
}