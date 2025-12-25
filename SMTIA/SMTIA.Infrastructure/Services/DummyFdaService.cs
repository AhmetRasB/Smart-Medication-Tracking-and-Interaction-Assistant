using SMTIA.Application.Services;

namespace SMTIA.Infrastructure.Services
{
    internal sealed class DummyFdaService : IFdaService
    {
        public Task<FdaSearchResponse> SearchMedicinesAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default)
        {
            var q = (searchTerm ?? "").Trim();
            var items = new List<FdaDrugLabel>
            {
                new(
                    Id: "dummy-1",
                    BrandName: "Parol",
                    GenericName: "Paracetamol",
                    ProductType: "human prescription drug",
                    Route: "oral",
                    DosageForms: new List<string> { "tablet" },
                    ActiveIngredients: new List<string> { "Paracetamol" },
                    Manufacturer: "Atabay",
                    Description: "Dummy veri: ağrı ve ateş için kullanılır.",
                    DosageAndAdministration: new List<string> { "Günde 1-2 kez" },
                    IndicationsAndUsage: new List<string> { "Ağrı / Ateş" },
                    BoxedWarning: null,
                    Warnings: new List<string> { "Doktorunuza danışın." },
                    Purpose: new List<string> { "Ağrı kesici / Ateş düşürücü" },
                    AdverseReactions: new List<string> { "Bulantı", "Baş dönmesi" },
                    OpenFda: null),
                new(
                    Id: "dummy-2",
                    BrandName: "Aspirin",
                    GenericName: "Acetylsalicylic Acid",
                    ProductType: "human OTC drug",
                    Route: "oral",
                    DosageForms: new List<string> { "tablet" },
                    ActiveIngredients: new List<string> { "Acetylsalicylic Acid" },
                    Manufacturer: "Bayer",
                    Description: "Dummy veri: ağrı kesici ve anti-inflamatuar.",
                    DosageAndAdministration: new List<string> { "Günde 1 kez" },
                    IndicationsAndUsage: new List<string> { "Ağrı" },
                    BoxedWarning: null,
                    Warnings: new List<string> { "Mide hassasiyeti olanlar dikkat." },
                    Purpose: new List<string> { "Ağrı kesici" },
                    AdverseReactions: new List<string> { "Mide rahatsızlığı" },
                    OpenFda: null),
                new(
                    Id: "dummy-3",
                    BrandName: "Nurofen",
                    GenericName: "Ibuprofen",
                    ProductType: "human OTC drug",
                    Route: "oral",
                    DosageForms: new List<string> { "capsule" },
                    ActiveIngredients: new List<string> { "Ibuprofen" },
                    Manufacturer: "Reckitt",
                    Description: "Dummy veri: ağrı ve iltihap için kullanılır.",
                    DosageAndAdministration: new List<string> { "Günde 1-3 kez" },
                    IndicationsAndUsage: new List<string> { "Ağrı / İltihap" },
                    BoxedWarning: null,
                    Warnings: new List<string> { "Mide kanaması riski." },
                    Purpose: new List<string> { "Anti-inflamatuar" },
                    AdverseReactions: new List<string> { "Mide yanması" },
                    OpenFda: null)
            };

            var filtered = string.IsNullOrWhiteSpace(q)
                ? items
                : items.Where(x =>
                        (!string.IsNullOrWhiteSpace(x.BrandName) && x.BrandName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(x.GenericName) && x.GenericName.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

            return Task.FromResult(new FdaSearchResponse(filtered.Take(limit).ToList(), filtered.Count));
        }

        public Task<FdaDrugLabel?> GetDrugLabelByIdAsync(string labelId, CancellationToken cancellationToken = default)
        {
            // Minimal dummy details
            var item = new FdaDrugLabel(
                Id: labelId,
                BrandName: "Dummy Medicine",
                GenericName: "Dummy Generic",
                ProductType: "human drug",
                Route: "oral",
                DosageForms: new List<string> { "tablet" },
                ActiveIngredients: new List<string> { "Dummy Ingredient" },
                Manufacturer: "Dummy Manufacturer",
                Description: "Bu veri OpenFDA yerine dummy olarak dönüyor.",
                DosageAndAdministration: new List<string> { "Günde 1-2 kez" },
                IndicationsAndUsage: new List<string> { "Ağrı/Ateş" },
                BoxedWarning: null,
                Warnings: new List<string> { "Doktorunuza danışın." },
                Purpose: new List<string> { "Bilgilendirme" },
                AdverseReactions: new List<string> { "Yan etkiler olabilir." },
                OpenFda: null);
            return Task.FromResult<FdaDrugLabel?>(item);
        }
    }
}


