using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Update
{
    public sealed record UpdateScheduleCommand(
        Guid Id,
        Guid UserId,
        string ScheduleName,
        DateTime StartDate,
        DateTime? EndDate,
        bool IsActive) : IRequest<Result<UpdateScheduleCommandResponse>>;
}

