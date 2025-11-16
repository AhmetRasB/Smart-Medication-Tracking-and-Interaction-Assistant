namespace SMTIA.Application.Features.Allergies.GetUserAllergies
{
    public sealed record GetUserAllergiesQueryResponse(
        Guid Id,
        string AllergyName,
        string? Description,
        string? Severity,
        DateTime CreatedAt);
}

