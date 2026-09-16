using FluentValidation;

namespace Membera.Auth.Application.Auth.SendEmailVerificationOtp;

public class SendEmailVerificationOtpCommandValidator : AbstractValidator<SendEmailVerificationOtpCommand>
{
    public SendEmailVerificationOtpCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
