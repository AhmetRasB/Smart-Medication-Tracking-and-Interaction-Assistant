using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.Search
{
    internal sealed class SearchLocalMedicinesQueryHandler(
        IRepository<Medicine> medicineRepository)
        : IRequestHandler<SearchLocalMedicinesQuery, Result<SearchLocalMedicinesQueryResponse>>
    {
        public async Task<Result<SearchLocalMedicinesQueryResponse>> Handle(
            SearchLocalMedicinesQuery request,
            CancellationToken cancellationToken)
        {
            var query = request.Query.Trim();

            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);

            var matches = allMedicines
                .Where(m =>
                    m.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(m.ActiveIngredient) &&
                     m.ActiveIngredient.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(m.Barcode) &&
                     m.Barcode.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var total = matches.Count;

            var items = matches
                .OrderBy(m => m.Name)
                .Take(request.Limit)
                .Select(m => new LocalMedicineDto(
                    m.Id,
                    m.Name,
                    m.ActiveIngredient,
                    m.DosageForm,
                    m.Manufacturer,
                    m.Barcode))
                .ToList();

            return new SearchLocalMedicinesQueryResponse(items, total);
        }
    }
}


