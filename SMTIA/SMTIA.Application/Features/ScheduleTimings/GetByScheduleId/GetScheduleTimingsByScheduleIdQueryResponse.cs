namespace SMTIA.Application.Features.ScheduleTimings.GetByScheduleId
{
    public sealed record GetScheduleTimingsByScheduleIdQueryResponse(
        List<ScheduleTimingDto> Timings);

    public sealed record ScheduleTimingDto(
        Guid Id,
        TimeOnly Time,
        decimal Dosage,
        string DosageUnit,
        int? DayOfWeek,
        bool IsActive);
}

