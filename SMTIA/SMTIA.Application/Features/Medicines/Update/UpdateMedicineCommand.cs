using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Update
{
    public sealed record UpdateMedicineCommand(
        Guid Id,
        string Name,
        string? Description,
        string? DosageForm,
        string? ActiveIngredient,
        string? Manufacturer,
        string? Barcode) : IRequest<Result<UpdateMedicineCommandResponse>>;
}

