using MediatR;
using TS.Result;

namespace SMTIA.Application.Features.Fda.SearchMedicines
{
    public sealed record SearchMedicinesQuery(
        string SearchTerm,
        int Limit = 10) : IRequest<Result<SearchMedicinesQueryResponse>>;
}

