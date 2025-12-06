using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Update
{
    internal sealed class UpdateScheduleTimingCommandHandler(
        IRepository<ScheduleTiming> timingRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateScheduleTimingCommand, Result<UpdateScheduleTimingCommandResponse>>
    {
        public async Task<Result<UpdateScheduleTimingCommandResponse>> Handle(UpdateScheduleTimingCommand request, CancellationToken cancellationToken)
        {
            var timing = await timingRepository.GetByIdAsync(request.Id, cancellationToken);

            if (timing == null)
            {
                return (404, "Zamanlama bulunamadı");
            }

            // Check if timing belongs to user's schedule
            var schedule = await scheduleRepository.GetByIdAsync(timing.MedicationScheduleId, cancellationToken);
            if (schedule == null)
            {
                return (404, "Takvim bulunamadı");
            }

            var prescription = await prescriptionRepository.GetByIdAsync(schedule.PrescriptionId, cancellationToken);
            if (prescription == null || prescription.UserId != request.UserId)
            {
                return (403, "Bu zamanlamayı güncelleme yetkiniz yok");
            }

            timing.Time = request.Time;
            timing.Dosage = request.Dosage;
            timing.DosageUnit = request.DosageUnit;
            timing.DayOfWeek = request.DayOfWeek;
            timing.IsActive = request.IsActive;

            await timingRepository.UpdateAsync(timing, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateScheduleTimingCommandResponse(timing.Id, "Zamanlama başarıyla güncellendi");
        }
    }
}
