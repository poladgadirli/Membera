using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Login;
using Membera.Auth.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.GoogleLogin;

public class GoogleLoginHandler
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<GoogleLoginHandler> _logger;

    public GoogleLoginHandler(
        IGoogleAuthService googleAuthService,
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<GoogleLoginHandler> logger)
    {
        _googleAuthService = googleAuthService;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<LoginResult> HandleAsync(GoogleLoginCommand command)
    {
        var googleUserInfo = await _googleAuthService.ValidateTokenAsync(command.IdToken);

        if (googleUserInfo is null)
        {
            _logger.LogWarning("Google login rejected: invalid Google token.");
            throw new InvalidOperationException("Invalid Google token.");
        }

        var user = await _userRepository.GetByEmailAsync(googleUserInfo.Email);

        var isNewUser = false;

        if (user is null)
        {
            user = User.CreateFromGoogle(
                googleUserInfo.FirstName,
                googleUserInfo.LastName,
                googleUserInfo.Email,
                googleUserInfo.GoogleId);

            await _userRepository.AddAsync(user);
            isNewUser = true;
        }
        else if (user.IsDeleted)
        {
            _logger.LogWarning("Google login rejected: account is deleted for email {Email}", googleUserInfo.Email);
            throw new InvalidOperationException("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        if (isNewUser)
            _logger.LogInformation("New user created via Google: {UserId}", user.Id);
        else
            _logger.LogInformation("Existing user logged in via Google: {UserId}", user.Id);

        return new LoginResult(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }
}
