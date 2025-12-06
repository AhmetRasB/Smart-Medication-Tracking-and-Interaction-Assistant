namespace SMTIA.Application.Features.Schedules.GetUserSchedules
{
    public sealed record GetUserSchedulesQueryResponse(
        List<ScheduleDto> Schedules);

    public sealed record ScheduleDto(
        Guid Id,
        Guid PrescriptionId,
        Guid PrescriptionMedicineId,
        string ScheduleName,
        DateTime StartDate,
        DateTime? EndDate,
        bool IsActive,
        int TimingCount);
}

