using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Interactions.Analyze
{
    public sealed record AnalyzeInteractionsCommand(
        Guid UserId,
        Guid? NewMedicineId,
        string? NewMedicineName) : IRequest<Result<AnalyzeInteractionsCommandResponse>>;
}

