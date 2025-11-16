using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ForgotPassword
{
    public sealed record ForgotPasswordCommand(
        string Email) : IRequest<Result<ForgotPasswordCommandResponse>>;
}

