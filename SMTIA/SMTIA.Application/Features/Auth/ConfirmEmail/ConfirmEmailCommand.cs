using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Auth.ConfirmEmail
{
    public sealed record ConfirmEmailCommand(
        string Email,
        string Token) : IRequest<Result<string>>;
}

