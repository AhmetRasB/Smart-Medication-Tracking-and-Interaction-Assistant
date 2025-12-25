namespace SMTIA.Application.Features.Medicines.Search
{
    public sealed record SearchLocalMedicinesQueryResponse(
        List<LocalMedicineDto> Medicines,
        int Total);

    public sealed record LocalMedicineDto(
        Guid Id,
        string Name,
        string? ActiveIngredient,
        string? DosageForm,
        string? Manufacturer,
        string? Barcode);
}


