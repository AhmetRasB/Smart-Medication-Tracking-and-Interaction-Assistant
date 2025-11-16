using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Prescriptions.Delete
{
    public sealed record DeletePrescriptionCommand(
        Guid Id) : IRequest<Result<string>>;
}

