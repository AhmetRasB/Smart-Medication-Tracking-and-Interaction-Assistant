namespace SMTIA.Application.Features.Calendar.GetCalendar
{
    public sealed record GetCalendarQueryResponse(
        List<CalendarItemDto> Items);

    public sealed record CalendarItemDto(
        Guid Id,
        Guid ScheduleId,
        Guid PrescriptionId,
        Guid PrescriptionMedicineId,
        string ScheduleName,
        string MedicineName,
        DateTime ScheduledTime,
        decimal Dosage,
        string DosageUnit,
        bool IsTaken,
        bool IsSkipped,
        DateTime? TakenTime,
        string? Notes);
}

