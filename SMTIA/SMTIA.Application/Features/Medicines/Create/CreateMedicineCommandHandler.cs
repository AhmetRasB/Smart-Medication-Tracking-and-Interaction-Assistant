using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Create
{
    internal sealed class CreateMedicineCommandHandler(
        IRepository<Medicine> medicineRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<CreateMedicineCommand, Result<CreateMedicineCommandResponse>>
    {
        public async Task<Result<CreateMedicineCommandResponse>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicine = new Medicine
            {
                Name = request.Name,
                Description = request.Description,
                DosageForm = request.DosageForm,
                ActiveIngredient = request.ActiveIngredient,
                Manufacturer = request.Manufacturer,
                Barcode = request.Barcode,
                CreatedAt = DateTime.UtcNow
            };

            await medicineRepository.AddAsync(medicine, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateMedicineCommandResponse(medicine.Id, "İlaç başarıyla oluşturuldu");
        }
    }
}

