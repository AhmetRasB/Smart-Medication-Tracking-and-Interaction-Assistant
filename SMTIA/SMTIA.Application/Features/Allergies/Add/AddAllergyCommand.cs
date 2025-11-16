using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Allergies.Add
{
    public sealed record AddAllergyCommand(
        Guid UserId,
        string AllergyName,
        string? Description,
        string? Severity) : IRequest<Result<AddAllergyCommandResponse>>;
}

