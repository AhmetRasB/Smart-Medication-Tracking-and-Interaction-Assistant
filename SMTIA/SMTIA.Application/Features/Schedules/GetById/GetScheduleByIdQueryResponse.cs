namespace SMTIA.Application.Features.Schedules.GetById
{
    public sealed record GetScheduleByIdQueryResponse(
        Guid Id,
        Guid PrescriptionId,
        Guid PrescriptionMedicineId,
        string ScheduleName,
        DateTime StartDate,
        DateTime? EndDate,
        bool IsActive,
        List<ScheduleTimingDto> Timings);

    public sealed record ScheduleTimingDto(
        Guid Id,
        TimeOnly Time,
        decimal Dosage,
        string DosageUnit,
        int? DayOfWeek,
        bool IsActive);
}

