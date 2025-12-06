using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.GetUserLogs
{
    public sealed record GetUserIntakeLogsQuery(
        Guid UserId,
        DateTime? StartDate,
        DateTime? EndDate,
        Guid? ScheduleId) : IRequest<Result<GetUserIntakeLogsQueryResponse>>;
}

