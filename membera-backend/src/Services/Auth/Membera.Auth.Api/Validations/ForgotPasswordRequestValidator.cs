using FluentValidation;
using Membera.Auth.Api.Controllers;

namespace Membera.Auth.Api.Validations;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
