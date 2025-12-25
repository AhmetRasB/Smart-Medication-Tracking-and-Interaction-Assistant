using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.MedicineMappings.Confirm
{
    internal sealed class ConfirmMedicineMappingCommandHandler(
        IRepository<MedicineMapping> mappingRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<ConfirmMedicineMappingCommand, Result<ConfirmMedicineMappingCommandResponse>>
    {
        public async Task<Result<ConfirmMedicineMappingCommandResponse>> Handle(
            ConfirmMedicineMappingCommand request,
            CancellationToken cancellationToken)
        {
            var mapping = await mappingRepository.GetByIdAsync(request.MappingId, cancellationToken);
            if (mapping == null)
            {
                return (404, "Mapping bulunamadı");
            }

            mapping.Status = request.Confirmed ? MappingStatus.Confirmed : MappingStatus.Rejected;
            mapping.Source = request.Confirmed ? MappingSource.UserConfirmed : mapping.Source;
            mapping.ConfirmedByUserId = request.Confirmed ? request.UserId : mapping.ConfirmedByUserId;
            mapping.UpdatedAt = DateTime.UtcNow;

            await mappingRepository.UpdateAsync(mapping, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var msg = request.Confirmed
                ? "Eşleme onaylandı (dataset büyütüldü)"
                : "Eşleme reddedildi";

            return new ConfirmMedicineMappingCommandResponse(mapping.Id, msg);
        }
    }
}


