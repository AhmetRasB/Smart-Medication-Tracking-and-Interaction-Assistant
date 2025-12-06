using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.GetUserLogs
{
    internal sealed class GetUserIntakeLogsQueryHandler(
        IRepository<IntakeLog> intakeLogRepository) : IRequestHandler<GetUserIntakeLogsQuery, Result<GetUserIntakeLogsQueryResponse>>
    {
        public async Task<Result<GetUserIntakeLogsQueryResponse>> Handle(GetUserIntakeLogsQuery request, CancellationToken cancellationToken)
        {
            var allLogs = await intakeLogRepository.ListAllAsync(cancellationToken);

            var query = allLogs.Where(l => l.UserId == request.UserId);

            if (request.ScheduleId.HasValue)
            {
                query = query.Where(l => l.MedicationScheduleId == request.ScheduleId.Value);
            }

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
                query = query.Where(l => l.ScheduledTime >= startDate);
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
                query = query.Where(l => l.ScheduledTime <= endDate);
            }

            var logs = query
                .OrderByDescending(l => l.ScheduledTime)
                .Select(l => new IntakeLogDto(
                    l.Id,
                    l.MedicationScheduleId,
                    l.ScheduledTime,
                    l.TakenTime,
                    l.IsTaken,
                    l.IsSkipped,
                    l.Notes,
                    l.CreatedAt))
                .ToList();

            return new GetUserIntakeLogsQueryResponse(logs);
        }
    }
}
