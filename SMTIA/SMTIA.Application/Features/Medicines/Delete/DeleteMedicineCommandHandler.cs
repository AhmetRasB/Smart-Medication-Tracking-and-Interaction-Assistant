using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Delete
{
    internal sealed class DeleteMedicineCommandHandler(
        IRepository<Medicine> medicineRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteMedicineCommand, Result<DeleteMedicineCommandResponse>>
    {
        public async Task<Result<DeleteMedicineCommandResponse>> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicine = await medicineRepository.GetByIdAsync(request.Id, cancellationToken);

            if (medicine == null)
            {
                return (404, "İlaç bulunamadı");
            }

            await medicineRepository.DeleteAsync(medicine, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteMedicineCommandResponse("İlaç başarıyla silindi");
        }
    }
}

