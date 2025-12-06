using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Create
{
    public sealed record CreateScheduleTimingCommand(
        Guid ScheduleId,
        Guid UserId,
        TimeOnly Time,
        decimal Dosage,
        string DosageUnit,
        int? DayOfWeek) : IRequest<Result<CreateScheduleTimingCommandResponse>>;
}

