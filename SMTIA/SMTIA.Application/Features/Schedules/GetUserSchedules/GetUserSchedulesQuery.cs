using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.GetUserSchedules
{
    public sealed record GetUserSchedulesQuery(Guid UserId) : IRequest<Result<GetUserSchedulesQueryResponse>>;
}

