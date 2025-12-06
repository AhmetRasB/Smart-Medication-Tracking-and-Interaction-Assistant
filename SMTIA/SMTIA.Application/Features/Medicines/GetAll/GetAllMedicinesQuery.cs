using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.GetAll
{
    public sealed record GetAllMedicinesQuery() : IRequest<Result<GetAllMedicinesQueryResponse>>;
}

