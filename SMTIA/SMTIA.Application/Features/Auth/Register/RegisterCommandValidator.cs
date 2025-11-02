using FluentValidation;

namespace SMTIA.Application.Features.Auth.Register
{
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(p => p.FirstName)
                .NotEmpty()
                .WithMessage("Ad alanı boş olamaz")
                .MinimumLength(2)
                .WithMessage("Ad en az 2 karakter olmalıdır");

            RuleFor(p => p.LastName)
                .NotEmpty()
                .WithMessage("Soyad alanı boş olamaz")
                .MinimumLength(2)
                .WithMessage("Soyad en az 2 karakter olmalıdır");

            RuleFor(p => p.Email)
                .NotEmpty()
                .WithMessage("Email alanı boş olamaz")
                .EmailAddress()
                .WithMessage("Geçerli bir email adresi giriniz");

            RuleFor(p => p.UserName)
                .NotEmpty()
                .WithMessage("Kullanıcı adı alanı boş olamaz")
                .MinimumLength(3)
                .WithMessage("Kullanıcı adı en az 3 karakter olmalıdır");

            RuleFor(p => p.Password)
                .NotEmpty()
                .WithMessage("Şifre alanı boş olamaz")
                .MinimumLength(1)
                .WithMessage("Şifre en az 1 karakter olmalıdır");

            RuleFor(p => p.Weight)
                .GreaterThan(0)
                .When(p => p.Weight.HasValue)
                .WithMessage("Kilo değeri 0'dan büyük olmalıdır");
        }
    }
}
