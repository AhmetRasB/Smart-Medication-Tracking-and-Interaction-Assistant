using FluentValidation;

namespace SMTIA.Application.Features.MedicineMappings.Confirm
{
    internal sealed class ConfirmMedicineMappingCommandValidator : AbstractValidator<ConfirmMedicineMappingCommand>
    {
        public ConfirmMedicineMappingCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x.MappingId)
                .NotEmpty().WithMessage("Mapping ID gereklidir");
        }
    }
}


