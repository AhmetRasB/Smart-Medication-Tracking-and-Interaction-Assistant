using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Delete
{
    public sealed record DeleteMedicineCommand(Guid Id) : IRequest<Result<DeleteMedicineCommandResponse>>;
}

