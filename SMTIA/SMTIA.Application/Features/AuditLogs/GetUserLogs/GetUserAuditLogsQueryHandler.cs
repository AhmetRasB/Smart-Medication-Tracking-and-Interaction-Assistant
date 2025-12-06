using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.AuditLogs.GetUserLogs
{
    internal sealed class GetUserAuditLogsQueryHandler(
        IRepository<AuditLog> auditLogRepository) : IRequestHandler<GetUserAuditLogsQuery, Result<GetUserAuditLogsQueryResponse>>
    {
        public async Task<Result<GetUserAuditLogsQueryResponse>> Handle(GetUserAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var allLogs = await auditLogRepository.ListAllAsync(cancellationToken);

            var query = allLogs.Where(l => l.UserId == request.UserId);

            if (request.StartDate.HasValue)
            {
                var startDate = request.StartDate.Value;
                if (startDate.Kind == DateTimeKind.Unspecified)
                {
                    startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
                }
                else
                {
                    startDate = startDate.ToUniversalTime();
                }
                query = query.Where(l => l.CreatedAt >= startDate);
            }

            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value;
                if (endDate.Kind == DateTimeKind.Unspecified)
                {
                    endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                }
                else
                {
                    endDate = endDate.ToUniversalTime();
                }
                query = query.Where(l => l.CreatedAt <= endDate);
            }

            if (!string.IsNullOrWhiteSpace(request.Action))
            {
                query = query.Where(l => l.Action.Equals(request.Action, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.EntityType))
            {
                query = query.Where(l => l.EntityType.Equals(request.EntityType, StringComparison.OrdinalIgnoreCase));
            }

            var logs = query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new AuditLogDto(
                    l.Id,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.RequestPath,
                    l.RequestMethod,
                    l.ResponseStatus,
                    l.IpAddress,
                    l.CreatedAt,
                    l.AdditionalData))
                .ToList();

            return new GetUserAuditLogsQueryResponse(logs);
        }
    }
}

