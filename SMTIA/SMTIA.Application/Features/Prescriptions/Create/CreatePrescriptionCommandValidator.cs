using FluentValidation;

namespace SMTIA.Application.Features.Prescriptions.Create
{
    internal sealed class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
    {
        public CreatePrescriptionCommandValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty()
                .WithMessage("Kullanıcı ID boş olamaz");

            RuleFor(p => p.PrescriptionDate)
                .NotEmpty()
                .WithMessage("Reçete tarihi boş olamaz");

            RuleFor(p => p.StartDate)
                .NotEmpty()
                .WithMessage("Başlangıç tarihi boş olamaz");

            RuleFor(p => p.EndDate)
                .GreaterThan(p => p.StartDate)
                .When(p => p.EndDate.HasValue)
                .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");

            RuleFor(p => p.Medicines)
                .NotEmpty()
                .WithMessage("En az bir ilaç eklenmelidir");

            RuleForEach(p => p.Medicines)
                .SetValidator(new PrescriptionMedicineDtoValidator());
        }
    }

    internal sealed class PrescriptionMedicineDtoValidator : AbstractValidator<PrescriptionMedicineDto>
    {
        public PrescriptionMedicineDtoValidator()
        {
            RuleFor(p => p.MedicineId)
                .NotEmpty()
                .WithMessage("İlaç ID boş olamaz");

            RuleFor(p => p.Dosage)
                .GreaterThan(0)
                .WithMessage("Doz miktarı 0'dan büyük olmalıdır");

            RuleFor(p => p.DosageUnit)
                .NotEmpty()
                .WithMessage("Doz birimi boş olamaz")
                .MaximumLength(20)
                .WithMessage("Doz birimi en fazla 20 karakter olabilir");

            RuleFor(p => p.Quantity)
                .GreaterThan(0)
                .WithMessage("Miktar 0'dan büyük olmalıdır");
        }
    }
}

