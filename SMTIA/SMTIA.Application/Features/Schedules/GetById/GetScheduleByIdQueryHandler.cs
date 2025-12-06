using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.GetById
{
    internal sealed class GetScheduleByIdQueryHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<ScheduleTiming> timingRepository,
        IRepository<UserPrescription> prescriptionRepository) : IRequestHandler<GetScheduleByIdQuery, Result<GetScheduleByIdQueryResponse>>
    {
        public async Task<Result<GetScheduleByIdQueryResponse>> Handle(GetScheduleByIdQuery request, CancellationToken cancellationToken)
        {
            var schedule = await scheduleRepository.GetByIdAsync(request.Id, cancellationToken);

            if (schedule == null)
            {
                return (404, "Takvim bulunamadı");
            }

            // Check if schedule belongs to user
            var prescription = await prescriptionRepository.GetByIdAsync(schedule.PrescriptionId, cancellationToken);
            if (prescription == null || prescription.UserId != request.UserId)
            {
                return (403, "Bu takvime erişim yetkiniz yok");
            }

            // Get timings
            var allTimings = await timingRepository.ListAllAsync(cancellationToken);
            var timings = allTimings
                .Where(t => t.MedicationScheduleId == schedule.Id)
                .Select(t => new ScheduleTimingDto(
                    t.Id,
                    t.Time,
                    t.Dosage,
                    t.DosageUnit,
                    t.DayOfWeek,
                    t.IsActive))
                .ToList();

            return new GetScheduleByIdQueryResponse(
                schedule.Id,
                schedule.PrescriptionId,
                schedule.PrescriptionMedicineId,
                schedule.ScheduleName,
                schedule.StartDate,
                schedule.EndDate,
                schedule.IsActive,
                timings);
        }
    }
}

