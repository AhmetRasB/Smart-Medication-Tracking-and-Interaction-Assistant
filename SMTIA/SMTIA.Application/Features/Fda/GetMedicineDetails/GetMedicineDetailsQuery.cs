using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Fda.GetMedicineDetails
{
    public sealed record GetMedicineDetailsQuery(string LabelId) : IRequest<Result<GetMedicineDetailsQueryResponse>>;
}

