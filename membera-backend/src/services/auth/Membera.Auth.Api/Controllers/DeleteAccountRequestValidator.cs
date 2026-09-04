using FluentValidation;

namespace Membera.Auth.Api.Controllers;

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
    }
}
