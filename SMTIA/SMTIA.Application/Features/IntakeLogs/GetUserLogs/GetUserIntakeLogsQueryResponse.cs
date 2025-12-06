namespace SMTIA.Application.Features.IntakeLogs.GetUserLogs
{
    public sealed record GetUserIntakeLogsQueryResponse(
        List<IntakeLogDto> Logs);

    public sealed record IntakeLogDto(
        Guid Id,
        Guid ScheduleId,
        DateTime ScheduledTime,
        DateTime? TakenTime,
        bool IsTaken,
        bool IsSkipped,
        string? Notes,
        DateTime CreatedAt);
}

