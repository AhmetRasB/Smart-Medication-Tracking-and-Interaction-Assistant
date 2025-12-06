using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Delete
{
    internal sealed class DeleteScheduleCommandHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteScheduleCommand, Result<DeleteScheduleCommandResponse>>
    {
        public async Task<Result<DeleteScheduleCommandResponse>> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
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
                return (403, "Bu takvimi silme yetkiniz yok");
            }

            await scheduleRepository.DeleteAsync(schedule, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteScheduleCommandResponse("Takvim başarıyla silindi");
        }
    }
}

