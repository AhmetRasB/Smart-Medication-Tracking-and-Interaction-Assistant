using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Diseases.GetUserDiseases
{
    public sealed record GetUserDiseasesQuery(
        Guid UserId) : IRequest<Result<List<GetUserDiseasesQueryResponse>>>;
}

