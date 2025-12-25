namespace SMTIA.Application.Features.Medicines.SmartSearch
{
    public sealed record SmartSearchMedicinesQueryResponse(
        List<LocalMedicineDto> LocalMedicines,
        int LocalTotal,
        MappingSuggestionDto? MappingSuggestion,
        List<OpenFdaMedicineDto>? OpenFdaMedicines,
        int? OpenFdaTotal);

    public sealed record LocalMedicineDto(
        Guid Id,
        string Name,
        string? ActiveIngredient,
        string? DosageForm,
        string? Manufacturer,
        string? Barcode,
        string? Description,
        List<SideEffectDto> SideEffects);

    public sealed record SideEffectDto(
        Guid Id,
        string Name,
        string? Description,
        string? Severity,
        string? Frequency);

    public sealed record MappingSuggestionDto(
        Guid? MappingId,
        string QueryTerm,
        string? BrandNameTr,
        string? ActiveIngredientTr,
        string? ActiveIngredientEn,
        decimal Confidence,
        string Status); // Pending/Confirmed/Rejected

    public sealed record OpenFdaMedicineDto(
        string Id,
        string? BrandName,
        string? GenericName,
        string? ProductType,
        string? Route,
        List<string>? DosageForms,
        List<string>? ActiveIngredients,
        string? Manufacturer);
}


