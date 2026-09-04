using FluentValidation;

namespace Membera.Auth.Application.Auth.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();
    }
}
