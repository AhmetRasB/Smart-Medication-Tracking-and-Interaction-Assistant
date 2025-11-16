using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ResetPassword
{
    public sealed record ResetPasswordCommand(
        string Email,
        string Token,
        string NewPassword) : IRequest<Result<ResetPasswordCommandResponse>>;
}

