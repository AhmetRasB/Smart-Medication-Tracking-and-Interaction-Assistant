using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.GetById
{
    public sealed record GetPrescriptionByIdQuery(
        Guid Id) : IRequest<Result<GetPrescriptionByIdQueryResponse>>;
}

