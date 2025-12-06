using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Calendar.GetCalendar
{
    public sealed record GetCalendarQuery(
        Guid UserId,
        DateTime StartDate,
        DateTime EndDate) : IRequest<Result<GetCalendarQueryResponse>>;
}

