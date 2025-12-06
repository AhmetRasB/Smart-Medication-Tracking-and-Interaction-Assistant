using System.Net.Http;
using MediatR;
using SMTIA.Application.Services;
using TS.Result;

namespace SMTIA.Application.Features.Fda.GetMedicineDetails
{
    internal sealed class GetMedicineDetailsQueryHandler(
        IFdaService fdaService) : IRequestHandler<GetMedicineDetailsQuery, Result<GetMedicineDetailsQueryResponse>>
    {
        public async Task<Result<GetMedicineDetailsQueryResponse>> Handle(GetMedicineDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var labelId = request.LabelId.Trim();
                var drugLabel = await fdaService.GetDrugLabelByIdAsync(labelId, cancellationToken);

                if (drugLabel == null)
                {
                    return (404, "İlaç bulunamadı");
                }

                return new GetMedicineDetailsQueryResponse(
                    drugLabel.Id,
                    drugLabel.BrandName,
                    drugLabel.GenericName,
                    drugLabel.ProductType,
                    drugLabel.Route,
                    drugLabel.DosageForms,
                    drugLabel.ActiveIngredients,
                    drugLabel.Manufacturer,
                    drugLabel.Description,
                    drugLabel.DosageAndAdministration,
                    drugLabel.IndicationsAndUsage,
                    drugLabel.BoxedWarning,
                    drugLabel.Warnings,
                    drugLabel.Purpose,
                    drugLabel.AdverseReactions,
                    drugLabel.OpenFda);
            }
            catch (HttpRequestException ex)
            {
                return (500, $"FDA servisinden ilaç detayı alınamadı: {ex.Message}");
            }
        }
    }
}

