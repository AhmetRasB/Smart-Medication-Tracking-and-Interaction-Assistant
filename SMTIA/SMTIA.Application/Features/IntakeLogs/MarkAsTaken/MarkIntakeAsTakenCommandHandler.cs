using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.MarkAsTaken
{
    internal sealed class MarkIntakeAsTakenCommandHandler(
        IRepository<IntakeLog> intakeLogRepository,
        IRepository<MedicationSchedule> scheduleRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<MarkIntakeAsTakenCommand, Result<MarkIntakeAsTakenCommandResponse>>
    {
        public async Task<Result<MarkIntakeAsTakenCommandResponse>> Handle(MarkIntakeAsTakenCommand request, CancellationToken cancellationToken)
        {
            var intakeLog = await intakeLogRepository.GetByIdAsync(request.LogId, cancellationToken);

            if (intakeLog == null)
            {
                return (404, "Alım kaydı bulunamadı");
            }

            if (intakeLog.UserId != request.UserId)
            {
                return (403, "Bu alım kaydını güncelleme yetkiniz yok");
            }

            if (request.IsTaken && request.IsSkipped)
            {
                return (400, "İlaç hem alındı hem atlandı olarak işaretlenemez");
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
            else if (request.IsTaken)
            {
                // If marked as taken but no time provided, use current time
                takenTimeUtc = DateTime.UtcNow;
            }

            intakeLog.TakenTime = takenTimeUtc;
            intakeLog.IsTaken = request.IsTaken;
            intakeLog.IsSkipped = request.IsSkipped;
            if (request.Notes != null)
            {
                intakeLog.Notes = request.Notes;
            }

            await intakeLogRepository.UpdateAsync(intakeLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var message = request.IsTaken 
                ? "İlaç alındı olarak işaretlendi" 
                : request.IsSkipped 
                    ? "İlaç atlandı olarak işaretlendi" 
                    : "Alım kaydı güncellendi";

            return new MarkIntakeAsTakenCommandResponse(intakeLog.Id, message);
        }
    }
}

