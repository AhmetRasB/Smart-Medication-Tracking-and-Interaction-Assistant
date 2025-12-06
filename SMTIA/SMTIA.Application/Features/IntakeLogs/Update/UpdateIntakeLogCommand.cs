using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Update
{
    public sealed record UpdateIntakeLogCommand(
        Guid Id,
        Guid UserId,
        DateTime? TakenTime,
        bool IsTaken,
        bool IsSkipped,
        string? Notes) : IRequest<Result<UpdateIntakeLogCommandResponse>>;
}

