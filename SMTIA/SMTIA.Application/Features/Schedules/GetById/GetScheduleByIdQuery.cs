using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.GetById
{
    public sealed record GetScheduleByIdQuery(Guid Id, Guid UserId) : IRequest<Result<GetScheduleByIdQueryResponse>>;
}

