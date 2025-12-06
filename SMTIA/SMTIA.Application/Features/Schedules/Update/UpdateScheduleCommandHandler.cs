using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Schedules.Update
{
    internal sealed class UpdateScheduleCommandHandler(
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateScheduleCommand, Result<UpdateScheduleCommandResponse>>
    {
        public async Task<Result<UpdateScheduleCommandResponse>> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
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
                return (403, "Bu takvimi güncelleme yetkiniz yok");
            }

            // Convert dates to UTC
            var startDate = request.StartDate;
            if (startDate.Kind == DateTimeKind.Unspecified)
            {
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }
            else
            {
                startDate = startDate.ToUniversalTime();
            }

            DateTime? endDateUtc = null;
            if (request.EndDate.HasValue)
            {
                var endDate = request.EndDate.Value;
                if (endDate.Kind == DateTimeKind.Unspecified)
                {
                    endDateUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
                }
                else
                {
                    endDateUtc = endDate.ToUniversalTime();
                }
            }

            schedule.ScheduleName = request.ScheduleName;
            schedule.StartDate = startDate;
            schedule.EndDate = endDateUtc;
            schedule.IsActive = request.IsActive;
            schedule.UpdatedAt = DateTime.UtcNow;

            await scheduleRepository.UpdateAsync(schedule, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateScheduleCommandResponse(schedule.Id, "Takvim başarıyla güncellendi");
        }
    }
}

