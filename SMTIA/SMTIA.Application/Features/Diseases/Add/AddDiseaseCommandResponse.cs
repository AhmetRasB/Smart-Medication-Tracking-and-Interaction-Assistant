namespace SMTIA.Application.Features.Diseases.Add
{
    public sealed record AddDiseaseCommandResponse(
        Guid DiseaseId,
        string Message);
}

