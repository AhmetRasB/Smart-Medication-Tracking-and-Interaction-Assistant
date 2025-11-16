using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.Delete
{
    internal sealed class DeletePrescriptionCommandHandler(
        IRepository<UserPrescription> prescriptionRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeletePrescriptionCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(DeletePrescriptionCommand request, CancellationToken cancellationToken)
        {
            var prescription = await prescriptionRepository.GetByIdAsync(request.Id, cancellationToken);

            if (prescription == null)
            {
                return (404, "Reçete bulunamadı");
            }

            await prescriptionRepository.DeleteAsync(prescription, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return "Reçete başarıyla silindi";
        }
    }
}

