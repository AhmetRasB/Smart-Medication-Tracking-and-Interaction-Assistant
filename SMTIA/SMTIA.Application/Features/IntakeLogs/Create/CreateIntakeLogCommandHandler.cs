using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Create
{
    internal sealed class CreateIntakeLogCommandHandler(
        IRepository<IntakeLog> intakeLogRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateIntakeLogCommand, Result<CreateIntakeLogCommandResponse>>
    {
        public async Task<Result<CreateIntakeLogCommandResponse>> Handle(CreateIntakeLogCommand request, CancellationToken cancellationToken)
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
                return (403, "Bu takvime alım kaydı ekleme yetkiniz yok");
            }

            // Convert dates to UTC
            var scheduledTime = request.ScheduledTime;
            if (scheduledTime.Kind == DateTimeKind.Unspecified)
            {
                scheduledTime = DateTime.SpecifyKind(scheduledTime, DateTimeKind.Utc);
            }
            else
            {
                scheduledTime = scheduledTime.ToUniversalTime();
            }

            DateTime? takenTimeUtc = null;
            if (request.TakenTime.HasValue)
            {
                var takenTime = request.TakenTime.Value;
                if (takenTime.Kind == DateTimeKind.Unspecified)
                {
                    takenTimeUtc = DateTime.SpecifyKind(takenTime, DateTimeKind.Utc);
                }
                else
                {
                    takenTimeUtc = takenTime.ToUniversalTime();
                }
            }

            var intakeLog = new IntakeLog
            {
                MedicationScheduleId = request.ScheduleId,
                UserId = request.UserId,
                ScheduledTime = scheduledTime,
                TakenTime = takenTimeUtc,
                IsTaken = request.IsTaken,
                IsSkipped = request.IsSkipped,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await intakeLogRepository.AddAsync(intakeLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateIntakeLogCommandResponse(intakeLog.Id, "Alım kaydı başarıyla oluşturuldu");
        }
    }
}
