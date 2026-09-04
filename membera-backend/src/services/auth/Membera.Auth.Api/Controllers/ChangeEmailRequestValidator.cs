using FluentValidation;

namespace Membera.Auth.Api.Controllers;

public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
    }
}
