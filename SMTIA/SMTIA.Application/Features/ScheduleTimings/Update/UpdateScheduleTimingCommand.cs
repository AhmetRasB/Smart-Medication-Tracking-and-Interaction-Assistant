using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Update
{
    public sealed record UpdateScheduleTimingCommand(
        Guid Id,
        Guid UserId,
        TimeOnly Time,
        decimal Dosage,
        string DosageUnit,
        int? DayOfWeek,
        bool IsActive) : IRequest<Result<UpdateScheduleTimingCommandResponse>>;
}

