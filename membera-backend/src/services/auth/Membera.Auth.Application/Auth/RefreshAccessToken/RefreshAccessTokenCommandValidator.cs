using FluentValidation;

namespace Membera.Auth.Application.Auth.RefreshAccessToken;

public class RefreshAccessTokenCommandValidator : AbstractValidator<RefreshAccessTokenCommand>
{
    public RefreshAccessTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
