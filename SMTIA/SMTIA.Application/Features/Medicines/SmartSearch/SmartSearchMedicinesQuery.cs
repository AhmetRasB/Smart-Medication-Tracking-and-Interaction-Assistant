using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.SmartSearch
{
    public sealed record SmartSearchMedicinesQuery(
        string Query,
        int Limit = 10,
        bool IncludeOpenFda = true) : IRequest<Result<SmartSearchMedicinesQueryResponse>>;
}


