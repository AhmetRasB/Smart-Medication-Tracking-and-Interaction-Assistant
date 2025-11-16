using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.GetUserPrescriptions
{
    public sealed record GetUserPrescriptionsQuery(
        Guid UserId) : IRequest<Result<List<GetUserPrescriptionsQueryResponse>>>;
}

