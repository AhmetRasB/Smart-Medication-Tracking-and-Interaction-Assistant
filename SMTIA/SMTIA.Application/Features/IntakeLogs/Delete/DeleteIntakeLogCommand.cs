using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.IntakeLogs.Delete
{
    public sealed record DeleteIntakeLogCommand(Guid Id, Guid UserId) : IRequest<Result<DeleteIntakeLogCommandResponse>>;
}

