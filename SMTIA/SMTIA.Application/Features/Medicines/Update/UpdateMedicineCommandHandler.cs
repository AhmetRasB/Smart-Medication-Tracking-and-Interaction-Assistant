using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Update
{
    internal sealed class UpdateMedicineCommandHandler(
        IRepository<Medicine> medicineRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateMedicineCommand, Result<UpdateMedicineCommandResponse>>
    {
        public async Task<Result<UpdateMedicineCommandResponse>> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicine = await medicineRepository.GetByIdAsync(request.Id, cancellationToken);

            if (medicine == null)
            {
                return (404, "İlaç bulunamadı");
            }

            medicine.Name = request.Name;
            medicine.Description = request.Description;
            medicine.DosageForm = request.DosageForm;
            medicine.ActiveIngredient = request.ActiveIngredient;
            medicine.Manufacturer = request.Manufacturer;
            medicine.Barcode = request.Barcode;
            medicine.UpdatedAt = DateTime.UtcNow;

            await medicineRepository.UpdateAsync(medicine, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateMedicineCommandResponse(medicine.Id, "İlaç başarıyla güncellendi");
        }
    }
}

