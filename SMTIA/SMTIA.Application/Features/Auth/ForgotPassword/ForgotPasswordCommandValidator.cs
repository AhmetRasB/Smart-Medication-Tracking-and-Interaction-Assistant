using FluentValidation;

namespace SMTIA.Application.Features.Auth.ForgotPassword
{
    public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty()
                .WithMessage("E-posta adresi gereklidir")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz");
        }
    }
}

