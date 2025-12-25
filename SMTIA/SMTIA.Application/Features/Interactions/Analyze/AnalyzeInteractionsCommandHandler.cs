using System.Text.Json;
using MediatR;
using SMTIA.Application.Abstractions;
using SMTIA.Application.Services;
using SMTIA.Domain.Entities;
using TS.Result;

namespace SMTIA.Application.Features.Interactions.Analyze
{
    internal sealed class AnalyzeInteractionsCommandHandler(
        IRepository<InteractionAnalysis> interactionAnalysisRepository,
        IRepository<UserPrescription> prescriptionRepository,
        IRepository<PrescriptionMedicine> prescriptionMedicineRepository,
        IRepository<Medicine> medicineRepository,
        IRepository<UserAllergy> allergyRepository,
        IPromptService promptService,
        IGemmaInteractionAnalyzer gemmaAnalyzer,
        IUnitOfWork unitOfWork) : IRequestHandler<AnalyzeInteractionsCommand, Result<AnalyzeInteractionsCommandResponse>>
    {
        public async Task<Result<AnalyzeInteractionsCommandResponse>> Handle(
            AnalyzeInteractionsCommand request,
            CancellationToken cancellationToken)
        {
            // Kullanıcının aktif reçetelerini ve ilaçlarını al
            var allPrescriptions = await prescriptionRepository.ListAllAsync(cancellationToken);
            var userPrescriptions = allPrescriptions
                .Where(p => p.UserId == request.UserId && p.IsActive && !p.IsDeleted)
                .ToList();

            var allPrescriptionMedicines = await prescriptionMedicineRepository.ListAllAsync(cancellationToken);
            var allMedicines = await medicineRepository.ListAllAsync(cancellationToken);

            var userMedicineIds = allPrescriptionMedicines
                .Where(pm => userPrescriptions.Select(p => p.Id).Contains(pm.PrescriptionId))
                .Select(pm => pm.MedicineId)
                .Distinct()
                .ToList();

            var userMedicines = allMedicines
                .Where(m => userMedicineIds.Contains(m.Id))
                .ToList();

            // Yeni ilaç bilgisini al
            string newMedicineName;
            Guid? newMedicineId = null;

            if (request.NewMedicineId.HasValue)
            {
                var medicine = await medicineRepository.GetByIdAsync(request.NewMedicineId.Value, cancellationToken);
                if (medicine == null)
                {
                    return (404, "İlaç bulunamadı");
                }
                newMedicineName = medicine.Name;
                newMedicineId = medicine.Id;
            }
            else if (!string.IsNullOrWhiteSpace(request.NewMedicineName))
            {
                newMedicineName = request.NewMedicineName;
            }
            else
            {
                return (400, "Yeni ilaç ID'si veya adı belirtilmelidir");
            }

            // Mevcut ilaçları JSON formatına çevir
            var existingMedicinesJson = JsonSerializer.Serialize(
                userMedicines.Select(m => new
                {
                    Name = m.Name,
                    ActiveIngredient = m.ActiveIngredient,
                    DosageForm = m.DosageForm,
                    Manufacturer = m.Manufacturer
                }).ToList());

            // Kullanıcının alerjilerini al
            var allAllergies = await allergyRepository.ListAllAsync(cancellationToken);
            var userAllergies = allAllergies
                .Where(a => a.UserId == request.UserId && !a.IsDeleted)
                .ToList();

            string? allergiesJson = null;
            if (userAllergies.Any())
            {
                allergiesJson = JsonSerializer.Serialize(
                    userAllergies.Select(a => new
                    {
                        Name = a.AllergyName,
                        Description = a.Description,
                        Severity = a.Severity
                    }).ToList());
            }

            // Prompt oluştur
            var prompt = promptService.CreateInteractionAnalysisPrompt(
                existingMedicinesJson,
                newMedicineName,
                allergiesJson);

            // Gemma'ya gönder ve yanıtı al
            string rawAiResponse;
            try
            {
                rawAiResponse = await gemmaAnalyzer.AnalyzeAsync(prompt, cancellationToken);
            }
            catch (Exception ex)
            {
                return (500, $"AI analizi sırasında hata oluştu: {ex.Message}");
            }

            // AI yanıtını parse et
            var analysisResult = ParseAiResponse(rawAiResponse);

            // InteractionAnalysis kaydı oluştur
            var interactionAnalysis = new InteractionAnalysis
            {
                UserId = request.UserId,
                NewMedicineId = newMedicineId,
                NewMedicineName = newMedicineName,
                ExistingMedicinesJson = existingMedicinesJson,
                AllergiesJson = allergiesJson,
                RiskLevel = analysisResult.RiskLevel,
                Summary = analysisResult.Summary,
                DetailedAnalysis = analysisResult.DetailedAnalysis,
                Recommendations = analysisResult.Recommendations,
                RawAiResponse = rawAiResponse,
                CreatedAt = DateTime.UtcNow
            };

            await interactionAnalysisRepository.AddAsync(interactionAnalysis, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new AnalyzeInteractionsCommandResponse(
                interactionAnalysis.Id,
                "İlaç etkileşim analizi başarıyla tamamlandı");
        }

        private (RiskLevel RiskLevel, string Summary, string? DetailedAnalysis, string? Recommendations) ParseAiResponse(string aiResponse)
        {
            try
            {
                // JSON içeriğini bulmaya çalış
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}');

                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var jsonDoc = JsonDocument.Parse(jsonContent);
                    var root = jsonDoc.RootElement;

                    var riskLevelStr = root.GetProperty("riskLevel").GetString() ?? "None";
                    var riskLevel = Enum.TryParse<RiskLevel>(riskLevelStr, ignoreCase: true, out var rl)
                        ? rl
                        : RiskLevel.None;

                    var summary = root.GetProperty("summary").GetString() ?? "Analiz tamamlandı.";
                    var detailedAnalysis = root.TryGetProperty("detailedAnalysis", out var da)
                        ? da.GetString()
                        : null;
                    var recommendations = root.TryGetProperty("recommendations", out var rec)
                        ? rec.GetString()
                        : null;

                    return (riskLevel, summary, detailedAnalysis, recommendations);
                }
            }
            catch
            {
                // JSON parse hatası durumunda varsayılan değerler döndür
            }

            // JSON bulunamazsa veya parse edilemezse, ham yanıtı summary olarak kullan
            return (RiskLevel.Medium, aiResponse, null, null);
        }
    }
}

