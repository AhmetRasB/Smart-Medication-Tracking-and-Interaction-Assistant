using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.GetById
{
    public sealed record GetMedicineByIdQuery(Guid Id) : IRequest<Result<GetMedicineByIdQueryResponse>>;
}

