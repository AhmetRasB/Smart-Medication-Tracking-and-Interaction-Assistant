using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.GetByScheduleId
{
    internal sealed class GetScheduleTimingsByScheduleIdQueryHandler(
        IRepository<ScheduleTiming> timingRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository) : IRequestHandler<GetScheduleTimingsByScheduleIdQuery, Result<GetScheduleTimingsByScheduleIdQueryResponse>>
    {
        public async Task<Result<GetScheduleTimingsByScheduleIdQueryResponse>> Handle(GetScheduleTimingsByScheduleIdQuery request, CancellationToken cancellationToken)
        {
            var schedule = await scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);

            if (schedule == null)
            {
                return (404, "Takvim bulunamadı");
            }

            // Check if schedule belongs to user
            var prescription = await prescriptionRepository.GetByIdAsync(schedule.PrescriptionId, cancellationToken);
            if (prescription == null || prescription.UserId != request.UserId)
            {
                return (403, "Bu takvimin zamanlamalarına erişim yetkiniz yok");
            }

            // Get timings
            var allTimings = await timingRepository.ListAllAsync(cancellationToken);
            var timings = allTimings
                .Where(t => t.MedicationScheduleId == request.ScheduleId)
                .Select(t => new ScheduleTimingDto(
                    t.Id,
                    t.Time,
                    t.Dosage,
                    t.DosageUnit,
                    t.DayOfWeek,
                    t.IsActive))
                .ToList();

            return new GetScheduleTimingsByScheduleIdQueryResponse(timings);
        }
    }
}
