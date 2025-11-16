using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Allergies.GetUserAllergies
{
    public sealed record GetUserAllergiesQuery(
        Guid UserId) : IRequest<Result<List<GetUserAllergiesQueryResponse>>>;
}

