using System.Text;

namespace SMTIA.Application.Services
{
    internal sealed class PromptService : IPromptService
    {
        public string CreateInteractionAnalysisPrompt(
            string existingMedicines,
            string newMedicineName,
            string? allergies = null)
        {
            var promptBuilder = new StringBuilder();

            // Rol tanımı
            promptBuilder.AppendLine("Sen bir farmakoloji uzmanısın. İlaç etkileşimlerini ve potansiyel riskleri analiz etme konusunda uzmanlaşmışsın.");
            promptBuilder.AppendLine();

            // Görev tanımı
            promptBuilder.AppendLine("Görevin: Bir hastanın mevcut ilaç listesi ile yeni eklenecek bir ilaç arasındaki potansiyel etkileşimleri analiz etmek ve risk seviyesini belirlemek.");
            promptBuilder.AppendLine();

            // Mevcut ilaçlar
            promptBuilder.AppendLine("HASTANIN MEVCUT İLAÇ LİSTESİ:");
            promptBuilder.AppendLine(existingMedicines);
            promptBuilder.AppendLine();

            // Yeni ilaç
            promptBuilder.AppendLine($"YENİ EKLENECEK İLAÇ: {newMedicineName}");
            promptBuilder.AppendLine();

            // Alerjiler (varsa)
            if (!string.IsNullOrWhiteSpace(allergies))
            {
                promptBuilder.AppendLine("HASTANIN BİLİNEN ALERJİLERİ:");
                promptBuilder.AppendLine(allergies);
                promptBuilder.AppendLine();
            }

            // Talimatlar
            promptBuilder.AppendLine("LÜTFEN AŞAĞIDAKİ BİLGİLERİ SAĞLA:");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("1. RİSK SEVİYESİ: None, Low, Medium, High veya Critical");
            promptBuilder.AppendLine("2. ÖZET: Kısa ve net bir özet (maksimum 200 kelime)");
            promptBuilder.AppendLine("3. DETAYLI ANALİZ: Etkileşimlerin detaylı açıklaması");
            promptBuilder.AppendLine("4. ÖNERİLER: Hasta için öneriler ve dikkat edilmesi gerekenler");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("YANITINI AŞAĞIDAKİ JSON FORMATINDA VER:");
            promptBuilder.AppendLine("{");
            promptBuilder.AppendLine("  \"riskLevel\": \"None|Low|Medium|High|Critical\",");
            promptBuilder.AppendLine("  \"summary\": \"Kısa özet metni\",");
            promptBuilder.AppendLine("  \"detailedAnalysis\": \"Detaylı analiz metni\",");
            promptBuilder.AppendLine("  \"recommendations\": \"Öneriler metni\"");
            promptBuilder.AppendLine("}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("ÖNEMLİ: Sadece JSON formatında yanıt ver. Başka açıklama ekleme.");

            return promptBuilder.ToString();
        }
    }
}

