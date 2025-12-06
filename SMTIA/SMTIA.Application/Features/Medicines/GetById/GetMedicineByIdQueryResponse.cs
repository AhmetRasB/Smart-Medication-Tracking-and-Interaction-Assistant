namespace SMTIA.Application.Features.Medicines.GetById
{
    public sealed record GetMedicineByIdQueryResponse(
        Guid Id,
        string Name,
        string? Description,
        string? DosageForm,
        string? ActiveIngredient,
        string? Manufacturer,
        string? Barcode,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}

