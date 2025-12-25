using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Application.Features.Fda.SearchMedicines;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Medicines.SmartSearch
{
    internal sealed class SmartSearchMedicinesQueryHandler(
        IRepository<Medicine> medicineRepository,
        IRepository<MedicineSideEffect> medicineSideEffectRepository,
        IRepository<SideEffect> sideEffectRepository,
        IFdaService fdaService)
        : IRequestHandler<SmartSearchMedicinesQuery, Result<SmartSearchMedicinesQueryResponse>>
    {
        public async Task<Result<SmartSearchMedicinesQueryResponse>> Handle(
            SmartSearchMedicinesQuery request,
            CancellationToken cancellationToken)
        {
            var q = request.Query.Trim();

            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);
            var localMatches = allMedicines
                .Where(m => !m.IsDeleted &&
                    (m.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrWhiteSpace(m.ActiveIngredient) &&
                      m.ActiveIngredient.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                     (!string.IsNullOrWhiteSpace(m.Barcode) &&
                      m.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            var localTotal = localMatches.Count;
            
            var allSideEffects = await sideEffectRepository.ListAllAsync(cancellationToken);
            var allMedicineSideEffects = await medicineSideEffectRepository.ListAllAsync(cancellationToken);

            var localItems = localMatches
                .OrderBy(m => m.Name)
                .Take(request.Limit)
                .Select(m =>
                {
                    // Bu ilacın yan etkilerini bul
                    var sideEffects = allMedicineSideEffects
                        .Where(mse => mse.MedicineId == m.Id)
                        .Select(mse =>
                        {
                            var se = allSideEffects.FirstOrDefault(s => s.Id == mse.SideEffectId);
                            return se != null
                                ? new SideEffectDto(
                                    se.Id,
                                    se.Name,
                                    se.Description,
                                    se.Severity,
                                    mse.Frequency)
                                : null;
                        })
                        .Where(se => se != null)
                        .Cast<SideEffectDto>()
                        .ToList();

                    return new LocalMedicineDto(
                        m.Id,
                        m.Name,
                        m.ActiveIngredient,
                        m.DosageForm,
                        m.Manufacturer,
                        m.Barcode,
                        m.Description,
                        sideEffects);
                })
                .ToList();

            // openFDA enrichment (opsiyonel)
            List<OpenFdaMedicineDto>? openFdaItems = null;
            int? openFdaTotal = null;

            if (request.IncludeOpenFda && localItems.Any())
            {
                // İlk ilacın etken maddesini kullan (İngilizce olmalı)
                var firstMedicine = localItems.First();
                if (!string.IsNullOrWhiteSpace(firstMedicine.ActiveIngredient))
                {
                    try
                    {
                        var fda = await fdaService.SearchMedicinesAsync(firstMedicine.ActiveIngredient, request.Limit, cancellationToken);
                        openFdaTotal = fda.Total;
                        openFdaItems = fda.Results.Select(r => new OpenFdaMedicineDto(
                            r.Id,
                            r.BrandName,
                            r.GenericName,
                            r.ProductType,
                            r.Route,
                            r.DosageForms,
                            r.ActiveIngredients,
                            r.Manufacturer)).ToList();
                    }
                    catch
                    {
                        // openFDA fail should not block local UX
                        openFdaItems = null;
                        openFdaTotal = null;
                    }
                }
            }

            return new SmartSearchMedicinesQueryResponse(
                localItems,
                localTotal,
                MappingSuggestion: null, // AI mapping kaldırıldı
                openFdaItems,
                openFdaTotal);
        }
    }
}


