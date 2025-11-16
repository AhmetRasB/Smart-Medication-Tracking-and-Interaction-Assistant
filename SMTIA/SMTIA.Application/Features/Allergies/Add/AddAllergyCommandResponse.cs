namespace SMTIA.Application.Features.Allergies.Add
{
    public sealed record AddAllergyCommandResponse(
        Guid AllergyId,
        string Message);
}

