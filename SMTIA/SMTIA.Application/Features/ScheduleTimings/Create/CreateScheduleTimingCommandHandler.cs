using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Create
{
    internal sealed class CreateScheduleTimingCommandHandler(
        IRepository<ScheduleTiming> timingRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateScheduleTimingCommand, Result<CreateScheduleTimingCommandResponse>>
    {
        public async Task<Result<CreateScheduleTimingCommandResponse>> Handle(CreateScheduleTimingCommand request, CancellationToken cancellationToken)
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
                return (403, "Bu takvime zamanlama ekleme yetkiniz yok");
            }

            var timing = new ScheduleTiming
            {
                MedicationScheduleId = request.ScheduleId,
                Time = request.Time,
                Dosage = request.Dosage,
                DosageUnit = request.DosageUnit,
                DayOfWeek = request.DayOfWeek,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await timingRepository.AddAsync(timing, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateScheduleTimingCommandResponse(timing.Id, "Zamanlama başarıyla eklendi");
        }
    }
}
