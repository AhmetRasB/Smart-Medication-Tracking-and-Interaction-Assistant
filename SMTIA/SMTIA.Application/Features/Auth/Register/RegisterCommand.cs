using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Auth.Register
{
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string UserName,
        string Password,
        DateTime? DateOfBirth,
        decimal? Weight) : IRequest<Result<RegisterCommandResponse>>;
}
