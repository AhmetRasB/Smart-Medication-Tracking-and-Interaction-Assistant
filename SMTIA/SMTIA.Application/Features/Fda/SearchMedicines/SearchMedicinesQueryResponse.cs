namespace SMTIA.Application.Features.Fda.SearchMedicines
{
    public sealed record SearchMedicinesQueryResponse(
        List<MedicineDto> Medicines,
        int Total);

    public sealed record MedicineDto(
        string Id,
        string? BrandName,
        string? GenericName,
        string? ProductType,
        string? Route,
        List<string>? DosageForms,
        List<string>? ActiveIngredients,
        string? Manufacturer);
}

