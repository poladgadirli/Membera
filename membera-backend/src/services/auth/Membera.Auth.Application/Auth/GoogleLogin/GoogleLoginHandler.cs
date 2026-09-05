using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Domain.Entities;

namespace Membera.Auth.Application.Auth.GoogleLogin;

public class GoogleLoginHandler
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public GoogleLoginHandler(
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResult> HandleAsync(GoogleLoginCommand command)
    {
        var googleUserInfo = await _googleAuthService.ValidateTokenAsync(command.IdToken);

        if (googleUserInfo is null)
            throw new InvalidOperationException("Invalid Google token.");

        var user = await _userRepository.GetByEmailAsync(googleUserInfo.Email);

        if (user is null)
        {
            user = User.CreateFromGoogle(
                googleUserInfo.FirstName,
                googleUserInfo.LastName,
                googleUserInfo.Email,
                googleUserInfo.GoogleId);

            await _userRepository.AddAsync(user);
        }
        else if (user.IsDeleted)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new LoginResult(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }
}