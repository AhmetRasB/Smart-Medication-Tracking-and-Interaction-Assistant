using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.GetById
{
    internal sealed class GetMedicineByIdQueryHandler(
        IRepository<Medicine> medicineRepository) : IRequestHandler<GetMedicineByIdQuery, Result<GetMedicineByIdQueryResponse>>
    {
        public async Task<Result<GetMedicineByIdQueryResponse>> Handle(GetMedicineByIdQuery request, CancellationToken cancellationToken)
        {
            var medicine = await medicineRepository.GetByIdAsync(request.Id, cancellationToken);

            if (medicine == null)
            {
                return (404, "İlaç bulunamadı");
            }

            return new GetMedicineByIdQueryResponse(
                medicine.Id,
                medicine.Name,
                medicine.Description,
                medicine.DosageForm,
                medicine.ActiveIngredient,
                medicine.Manufacturer,
                medicine.Barcode,
                medicine.CreatedAt,
                medicine.UpdatedAt);
        }
    }
}

