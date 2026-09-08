using Membera.Auth.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Membera.Auth.Application.Auth.Login;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning("Failed login attempt: no user found for email {Email}", command.Email);
            throw new InvalidOperationException("Invalid email or password.");
        }

        if (user.IsDeleted)
        {
            _logger.LogWarning("Failed login attempt: account is deleted for email {Email}", command.Email);
            throw new InvalidOperationException("Invalid email or password.");
        }

        if (user.PasswordHash is null)
        {
            _logger.LogWarning("Failed login attempt: Google-only account attempting local login for email {Email}", command.Email);
            throw new InvalidOperationException("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Failed login attempt: wrong password for email {Email}", command.Email);
            throw new InvalidOperationException("Invalid email or password.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        _logger.LogInformation("Successful login for UserId: {UserId}, Email: {Email}", user.Id, user.Email);

        return new LoginResult(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }
}
