using FluentValidation;

namespace SMTIA.Application.Features.ScheduleTimings.Create
{
    internal sealed class CreateScheduleTimingCommandValidator : AbstractValidator<CreateScheduleTimingCommand>
    {
        public CreateScheduleTimingCommandValidator()
        {
            RuleFor(x => x.ScheduleId)
                .NotEmpty().WithMessage("Takvim ID gereklidir");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x.Dosage)
                .GreaterThan(0).WithMessage("Doz miktarı 0'dan büyük olmalıdır");

            RuleFor(x => x.DosageUnit)
                .NotEmpty().WithMessage("Doz birimi gereklidir")
                .MaximumLength(20).WithMessage("Doz birimi en fazla 20 karakter olabilir");

            RuleFor(x => x.DayOfWeek)
                .InclusiveBetween(0, 6).When(x => x.DayOfWeek.HasValue)
                .WithMessage("Haftanın günü 0-6 arasında olmalıdır (0=Pazar, 6=Cumartesi)");
        }
    }
}

