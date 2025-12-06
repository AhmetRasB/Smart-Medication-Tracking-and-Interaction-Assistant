using FluentValidation;

namespace SMTIA.Application.Features.IntakeLogs.Create
{
    internal sealed class CreateIntakeLogCommandValidator : AbstractValidator<CreateIntakeLogCommand>
    {
        public CreateIntakeLogCommandValidator()
        {
            RuleFor(x => x.ScheduleId)
                .NotEmpty().WithMessage("Takvim ID gereklidir");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x.ScheduledTime)
                .NotEmpty().WithMessage("Planlanan zaman gereklidir");

            RuleFor(x => x)
                .Must(x => !(x.IsTaken && x.IsSkipped))
                .WithMessage("İlaç hem alındı hem atlandı olarak işaretlenemez");
        }
    }
}

