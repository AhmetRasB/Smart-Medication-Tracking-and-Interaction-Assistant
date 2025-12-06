using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.GetByScheduleId
{
    public sealed record GetScheduleTimingsByScheduleIdQuery(Guid ScheduleId, Guid UserId) : IRequest<Result<GetScheduleTimingsByScheduleIdQueryResponse>>;
}

