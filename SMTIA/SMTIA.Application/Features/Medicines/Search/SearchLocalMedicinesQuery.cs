using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Search
{
    public sealed record SearchLocalMedicinesQuery(
        string Query,
        int Limit = 10) : IRequest<Result<SearchLocalMedicinesQueryResponse>>;
}


