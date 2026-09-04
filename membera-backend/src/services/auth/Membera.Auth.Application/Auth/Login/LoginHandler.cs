using Membera.Auth.Application.Abstractions;

namespace Membera.Auth.Application.Auth.Login;

public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResult> HandleAsync(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email);
        if (user is null)
            throw new InvalidOperationException("Email və ya şifrə yanlışdır.");

        var isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidOperationException("Email və ya şifrə yanlışdır.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken);

        return new LoginResult(accessToken, refreshToken.Token, DateTime.UtcNow.AddMinutes(15));
    }
}