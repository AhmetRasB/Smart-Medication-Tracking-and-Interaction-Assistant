namespace SMTIA.Application.Features.Medicines.GetAll
{
    public sealed record GetAllMedicinesQueryResponse(
        List<MedicineDto> Medicines);

    public sealed record MedicineDto(
        Guid Id,
        string Name,
        string? Description,
        string? DosageForm,
        string? ActiveIngredient,
        string? Manufacturer,
        string? Barcode,
        DateTime CreatedAt);
}

