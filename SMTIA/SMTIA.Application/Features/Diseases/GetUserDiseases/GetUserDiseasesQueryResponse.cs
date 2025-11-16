namespace SMTIA.Application.Features.Diseases.GetUserDiseases
{
    public sealed record GetUserDiseasesQueryResponse(
        Guid Id,
        string DiseaseName,
        string? Description,
        DateTime? DiagnosisDate,
        bool IsActive,
        DateTime CreatedAt);
}

