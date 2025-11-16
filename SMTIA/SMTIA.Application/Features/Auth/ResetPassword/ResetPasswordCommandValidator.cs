using FluentValidation;

namespace SMTIA.Application.Features.Auth.ResetPassword
{
    public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty()
                .WithMessage("E-posta adresi gereklidir")
                .EmailAddress()
                .WithMessage("Geçerli bir e-posta adresi giriniz");

            RuleFor(p => p.Token)
                .NotEmpty()
                .WithMessage("Token gereklidir");

            RuleFor(p => p.NewPassword)
                .NotEmpty()
                .WithMessage("Yeni şifre gereklidir")
                .MinimumLength(6)
                .WithMessage("Şifre en az 6 karakter olmalıdır");
        }
    }
}

