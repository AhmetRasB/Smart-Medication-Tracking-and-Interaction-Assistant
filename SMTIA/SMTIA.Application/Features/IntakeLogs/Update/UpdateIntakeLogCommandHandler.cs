using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Update
{
    internal sealed class UpdateIntakeLogCommandHandler(
        IRepository<IntakeLog> intakeLogRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateIntakeLogCommand, Result<UpdateIntakeLogCommandResponse>>
    {
        public async Task<Result<UpdateIntakeLogCommandResponse>> Handle(UpdateIntakeLogCommand request, CancellationToken cancellationToken)
        {
            var intakeLog = await intakeLogRepository.GetByIdAsync(request.Id, cancellationToken);

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

            intakeLog.TakenTime = takenTimeUtc;
            intakeLog.IsTaken = request.IsTaken;
            intakeLog.IsSkipped = request.IsSkipped;
            intakeLog.Notes = request.Notes;

            await intakeLogRepository.UpdateAsync(intakeLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateIntakeLogCommandResponse(intakeLog.Id, "Alım kaydı başarıyla güncellendi");
        }
    }
}
