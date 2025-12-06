using MediatR;
using SMTIA.Application.Services;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Create
{
    public sealed record CreateScheduleCommand(
        Guid UserId,
        Guid PrescriptionId,
        Guid PrescriptionMedicineId,
        string ScheduleName,
        DateTime StartDate,
        DateTime? EndDate,
        decimal Dosage,
        string DosageUnit,
        ScheduleTimingRuleDto TimingRule) : IRequest<Result<CreateScheduleCommandResponse>>;

    /// <summary>
    /// Zamanlama kuralı DTO - kullanıcıdan gelen kuralları temsil eder
    /// </summary>
    public sealed record ScheduleTimingRuleDto(
        ScheduleTimingType Type,
        int? IntervalHours,
        List<int>? DaysOfWeek,
        TimeOnly? Time,
        List<TimeOnly>? DailyTimes);
}

