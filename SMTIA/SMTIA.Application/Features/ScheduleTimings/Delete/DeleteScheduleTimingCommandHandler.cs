using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.ScheduleTimings.Delete
{
    internal sealed class DeleteScheduleTimingCommandHandler(
        IRepository<ScheduleTiming> timingRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteScheduleTimingCommand, Result<DeleteScheduleTimingCommandResponse>>
    {
        public async Task<Result<DeleteScheduleTimingCommandResponse>> Handle(DeleteScheduleTimingCommand request, CancellationToken cancellationToken)
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
                return (403, "Bu zamanlamayı silme yetkiniz yok");
            }

            await timingRepository.DeleteAsync(timing, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteScheduleTimingCommandResponse("Zamanlama başarıyla silindi");
        }
    }
}
