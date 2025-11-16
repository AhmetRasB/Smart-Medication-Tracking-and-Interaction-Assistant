using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Diseases.Add
{
    public sealed record AddDiseaseCommand(
        Guid UserId,
        string DiseaseName,
        string? Description,
        DateTime? DiagnosisDate) : IRequest<Result<AddDiseaseCommandResponse>>;
}

