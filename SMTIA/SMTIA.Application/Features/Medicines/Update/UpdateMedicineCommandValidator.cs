using FluentValidation;

namespace SMTIA.Application.Features.Medicines.Update
{
    internal sealed class UpdateMedicineCommandValidator : AbstractValidator<UpdateMedicineCommand>
    {
        public UpdateMedicineCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("İlaç ID gereklidir");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("İlaç adı gereklidir")
                .MaximumLength(200).WithMessage("İlaç adı en fazla 200 karakter olabilir");
        }
    }
}

