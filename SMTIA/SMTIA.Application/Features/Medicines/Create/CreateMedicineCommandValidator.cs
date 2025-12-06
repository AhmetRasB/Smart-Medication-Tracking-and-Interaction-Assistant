using FluentValidation;

namespace SMTIA.Application.Features.Medicines.Create
{
    internal sealed class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
    {
        public CreateMedicineCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("İlaç adı gereklidir")
                .MaximumLength(200).WithMessage("İlaç adı en fazla 200 karakter olabilir");
        }
    }
}

