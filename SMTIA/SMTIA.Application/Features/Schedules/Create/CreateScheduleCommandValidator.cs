using FluentValidation;
using SMTIA.Application.Services;

namespace SMTIA.Application.Features.Schedules.Create
{
    internal sealed class CreateScheduleCommandValidator : AbstractValidator<CreateScheduleCommand>
    {
        public CreateScheduleCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı ID gereklidir");

            RuleFor(x => x.PrescriptionId)
                .NotEmpty().WithMessage("Reçete ID gereklidir");

            RuleFor(x => x.PrescriptionMedicineId)
                .NotEmpty().WithMessage("Reçete ilaç ID gereklidir");

            RuleFor(x => x.ScheduleName)
                .NotEmpty().WithMessage("Takvim adı gereklidir")
                .MaximumLength(200).WithMessage("Takvim adı en fazla 200 karakter olabilir");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Başlangıç tarihi gereklidir");

            RuleFor(x => x.Dosage)
                .GreaterThan(0).WithMessage("Doz miktarı 0'dan büyük olmalıdır");

            RuleFor(x => x.DosageUnit)
                .NotEmpty().WithMessage("Doz birimi gereklidir")
                .MaximumLength(20).WithMessage("Doz birimi en fazla 20 karakter olabilir");

            RuleFor(x => x.TimingRule)
                .NotNull().WithMessage("Zamanlama kuralı gereklidir");

            RuleFor(x => x.TimingRule)
                .Must(BeValidTimingRule).WithMessage("Zamanlama kuralı geçersiz");

            // Interval tipi için IntervalHours kontrolü
            When(x => x.TimingRule != null && x.TimingRule.Type == ScheduleTimingType.Interval, () =>
            {
                RuleFor(x => x.TimingRule!.IntervalHours)
                    .NotNull().WithMessage("Interval tipi için IntervalHours gereklidir")
                    .GreaterThan(0).WithMessage("IntervalHours 0'dan büyük olmalıdır");
            });

            // Weekly tipi için DaysOfWeek ve Time kontrolü
            When(x => x.TimingRule != null && x.TimingRule.Type == ScheduleTimingType.Weekly, () =>
            {
                RuleFor(x => x.TimingRule!.DaysOfWeek)
                    .NotNull().WithMessage("Weekly tipi için DaysOfWeek gereklidir")
                    .NotEmpty().WithMessage("En az bir gün belirtilmelidir");

                RuleForEach(x => x.TimingRule!.DaysOfWeek)
                    .InclusiveBetween(0, 6).WithMessage("Haftanın günü 0-6 arasında olmalıdır (0=Pazar, 6=Cumartesi)");

                RuleFor(x => x.TimingRule!.Time)
                    .NotNull().WithMessage("Weekly tipi için Time gereklidir");
            });

            // Daily tipi için DailyTimes kontrolü
            When(x => x.TimingRule != null && x.TimingRule.Type == ScheduleTimingType.Daily, () =>
            {
                RuleFor(x => x.TimingRule!.DailyTimes)
                    .NotNull().WithMessage("Daily tipi için DailyTimes gereklidir")
                    .NotEmpty().WithMessage("En az bir saat belirtilmelidir");
            });
        }

        private bool BeValidTimingRule(ScheduleTimingRuleDto? rule)
        {
            if (rule == null) return false;

            return rule.Type switch
            {
                ScheduleTimingType.Interval => rule.IntervalHours.HasValue && rule.IntervalHours.Value > 0,
                ScheduleTimingType.Weekly => rule.DaysOfWeek != null && rule.DaysOfWeek.Any() && rule.Time.HasValue,
                ScheduleTimingType.Daily => rule.DailyTimes != null && rule.DailyTimes.Any(),
                _ => false
            };
        }
    }
}

