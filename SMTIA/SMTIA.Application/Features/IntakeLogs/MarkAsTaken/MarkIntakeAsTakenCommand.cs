using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.MarkAsTaken
{
    public sealed record MarkIntakeAsTakenCommand(
        Guid LogId,
        Guid UserId,
        bool IsTaken,
        bool IsSkipped,
        DateTime? TakenTime,
        string? Notes) : IRequest<Result<MarkIntakeAsTakenCommandResponse>>;
}

