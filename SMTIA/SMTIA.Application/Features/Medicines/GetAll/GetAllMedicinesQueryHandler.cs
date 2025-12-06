using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.GetAll
{
    internal sealed class GetAllMedicinesQueryHandler(
        IRepository<Medicine> medicineRepository) : IRequestHandler<GetAllMedicinesQuery, Result<GetAllMedicinesQueryResponse>>
    {
        public async Task<Result<GetAllMedicinesQueryResponse>> Handle(GetAllMedicinesQuery request, CancellationToken cancellationToken)
        {
            var medicines = await medicineRepository.ListAllAsync(cancellationToken);

            var medicineDtos = medicines.Select(m => new MedicineDto(
                m.Id,
                m.Name,
                m.Description,
                m.DosageForm,
                m.ActiveIngredient,
                m.Manufacturer,
                m.Barcode,
                m.CreatedAt)).ToList();

            return new GetAllMedicinesQueryResponse(medicineDtos);
        }
    }
}

