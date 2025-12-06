using System.Net.Http;
using MediatR;
using SMTIA.Application.Services;
using TS.Result;

namespace SMTIA.Application.Features.Fda.SearchMedicines
{
    internal sealed class SearchMedicinesQueryHandler(
        IFdaService fdaService) : IRequestHandler<SearchMedicinesQuery, Result<SearchMedicinesQueryResponse>>
    {
        public async Task<Result<SearchMedicinesQueryResponse>> Handle(SearchMedicinesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var trimmedTerm = request.SearchTerm.Trim();
                var fdaResponse = await fdaService.SearchMedicinesAsync(trimmedTerm, request.Limit, cancellationToken);

                var medicines = fdaResponse.Results.Select(m => new MedicineDto(
                    m.Id,
                    m.BrandName,
                    m.GenericName,
                    m.ProductType,
                    m.Route,
                    m.DosageForms,
                    m.ActiveIngredients,
                    m.Manufacturer)).ToList();

                return new SearchMedicinesQueryResponse(medicines, fdaResponse.Total);
            }
            catch (HttpRequestException ex)
            {
                return (500, $"FDA servisinden veri alınamadı: {ex.Message}");
            }
        }
    }
}

