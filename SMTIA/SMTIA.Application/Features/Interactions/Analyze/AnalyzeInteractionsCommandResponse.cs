namespace SMTIA.Application.Features.Interactions.Analyze
{
    public sealed record AnalyzeInteractionsCommandResponse(
        Guid AnalysisId,
        string Message);
}

