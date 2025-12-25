using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.MedicineMappings.Confirm
{
    public sealed record ConfirmMedicineMappingCommand(
        Guid UserId,
        Guid MappingId,
        bool Confirmed) : IRequest<Result<ConfirmMedicineMappingCommandResponse>>;
}


