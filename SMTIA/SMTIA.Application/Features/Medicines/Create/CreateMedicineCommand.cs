using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Create
{
    public sealed record CreateMedicineCommand(
        string Name,
        string? Description,
        string? DosageForm,
        string? ActiveIngredient,
        string? Manufacturer,
        string? Barcode) : IRequest<Result<CreateMedicineCommandResponse>>;
}

