using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Create
{
    public sealed record CreateIntakeLogCommand(
        Guid ScheduleId,
        Guid UserId,
        DateTime ScheduledTime,
        DateTime? TakenTime,
        bool IsTaken,
        bool IsSkipped,
        string? Notes) : IRequest<Result<CreateIntakeLogCommandResponse>>;
}

