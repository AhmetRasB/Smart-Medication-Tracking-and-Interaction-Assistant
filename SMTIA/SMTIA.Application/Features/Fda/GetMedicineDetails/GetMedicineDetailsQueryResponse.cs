namespace SMTIA.Application.Features.Fda.GetMedicineDetails
{
    public sealed record GetMedicineDetailsQueryResponse(
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

