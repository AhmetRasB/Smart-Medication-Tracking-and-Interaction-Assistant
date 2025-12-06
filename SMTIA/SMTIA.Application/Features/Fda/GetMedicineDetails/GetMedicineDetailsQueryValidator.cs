using FluentValidation;

namespace SMTIA.Application.Features.Fda.GetMedicineDetails
{
    internal sealed class GetMedicineDetailsQueryValidator : AbstractValidator<GetMedicineDetailsQuery>
    {
        public GetMedicineDetailsQueryValidator()
        {
            RuleFor(x => x.LabelId)
                .NotEmpty().WithMessage("LabelId zorunludur");
        }
    }
}

