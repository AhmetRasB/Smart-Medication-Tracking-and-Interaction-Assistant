using FluentValidation;

namespace SMTIA.Application.Features.Medicines.AddMedicineToUser
{
    internal sealed class AddMedicineToUserCommandValidator : AbstractValidator<AddMedicineToUserCommand>
    {
        public AddMedicineToUserCommandValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty()
                .WithMessage("Kullanıcı ID boş olamaz");

            RuleFor(p => p.MedicineName)
                .NotEmpty()
                .WithMessage("İlaç adı boş olamaz")
                .MaximumLength(200)
                .WithMessage("İlaç adı en fazla 200 karakter olabilir");

            RuleFor(p => p.Dosage)
                .GreaterThan(0)
                .WithMessage("Doz miktarı 0'dan büyük olmalıdır");

            RuleFor(p => p.DosageUnit)
                .NotEmpty()
                .WithMessage("Doz birimi boş olamaz")
                .MaximumLength(20)
                .WithMessage("Doz birimi en fazla 20 karakter olabilir");

            RuleFor(p => p.PackageSize)
                .GreaterThan(0)
                .WithMessage("Paket boyutu 0'dan büyük olmalıdır");

            RuleFor(p => p.DailyDoseCount)
                .GreaterThan(0)
                .WithMessage("Günlük doz sayısı 0'dan büyük olmalıdır");
        }
    }
}

