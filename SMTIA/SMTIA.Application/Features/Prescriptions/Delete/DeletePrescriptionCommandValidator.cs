using FluentValidation;

namespace SMTIA.Application.Features.Prescriptions.Delete
{
    internal sealed class DeletePrescriptionCommandValidator : AbstractValidator<DeletePrescriptionCommand>
    {
        public DeletePrescriptionCommandValidator()
        {
            RuleFor(p => p.Id)
                .NotEmpty()
                .WithMessage("Reçete ID boş olamaz");
        }
    }
}

