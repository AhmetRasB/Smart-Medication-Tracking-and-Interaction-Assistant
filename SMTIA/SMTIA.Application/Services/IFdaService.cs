namespace SMTIA.Application.Services
{
    public interface IFdaService
    {
        Task<FdaSearchResponse> SearchMedicinesAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default);
        Task<FdaDrugLabel?> GetDrugLabelByIdAsync(string labelId, CancellationToken cancellationToken = default);
    }

    public sealed record FdaSearchResponse(
        List<FdaDrugLabel> Results,
        int Total);

    public sealed record FdaDrugLabel(
        string Id,
        string? BrandName,
        string? GenericName,
        string? ProductType,
        string? Route,
        List<string>? DosageForms,
        List<string>? ActiveIngredients,
        string? Manufacturer,
        string? Description,
        List<string>? DosageAndAdministration,
        List<string>? IndicationsAndUsage,
        List<string>? BoxedWarning,
        List<string>? Warnings,
        List<string>? Purpose,
        List<string>? AdverseReactions,
        Dictionary<string, object>? OpenFda);
}

