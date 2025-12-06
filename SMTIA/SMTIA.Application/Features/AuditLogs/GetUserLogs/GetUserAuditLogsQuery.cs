using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.AuditLogs.GetUserLogs
{
    public sealed record GetUserAuditLogsQuery(
        Guid UserId,
        DateTime? StartDate,
        DateTime? EndDate,
        string? Action,
        string? EntityType) : IRequest<Result<GetUserAuditLogsQueryResponse>>;
}

