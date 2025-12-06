using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Delete
{
    public sealed record DeleteScheduleCommand(Guid Id, Guid UserId) : IRequest<Result<DeleteScheduleCommandResponse>>;
}

