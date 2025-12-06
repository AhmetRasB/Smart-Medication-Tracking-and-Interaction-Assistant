using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Delete
{
    public sealed record DeleteScheduleTimingCommand(Guid Id, Guid UserId) : IRequest<Result<DeleteScheduleTimingCommandResponse>>;
}

