using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Delete
{
    internal sealed class DeleteIntakeLogCommandHandler(
        IRepository<IntakeLog> intakeLogRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteIntakeLogCommand, Result<DeleteIntakeLogCommandResponse>>
    {
        public async Task<Result<DeleteIntakeLogCommandResponse>> Handle(DeleteIntakeLogCommand request, CancellationToken cancellationToken)
        {
            var intakeLog = await intakeLogRepository.GetByIdAsync(request.Id, cancellationToken);

            if (intakeLog == null)
            {
                return (404, "Alım kaydı bulunamadı");
            }

            if (intakeLog.UserId != request.UserId)
            {
                return (403, "Bu alım kaydını silme yetkiniz yok");
            }

            await intakeLogRepository.DeleteAsync(intakeLog, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteIntakeLogCommandResponse("Alım kaydı başarıyla silindi");
        }
    }
}
