using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Auth.Register
{
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string UserName,
        string Password,
        DateTime? DateOfBirth,
        decimal? Weight,
        string? BloodType,
        List<AllergyDto>? Allergies,
        List<DiseaseDto>? Diseases) : IRequest<Result<RegisterCommandResponse>>;

    public sealed record AllergyDto(
        string AllergyName,
        string? Description,
        string? Severity);

    public sealed record DiseaseDto(
        string DiseaseName,
        string? Description,
        DateTime? DiagnosisDate);
}
