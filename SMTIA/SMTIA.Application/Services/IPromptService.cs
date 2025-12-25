namespace SMTIA.Application.Services
{
    /// <summary>
    /// AI modeline gönderilecek prompt'ları oluşturan servis
    /// </summary>
    public interface IPromptService
    {
        /// <summary>
        /// İlaç etkileşim analizi için prompt oluşturur
        /// </summary>
        /// <param name="existingMedicines">Mevcut ilaç listesi (JSON formatında)</param>
        /// <param name="newMedicineName">Yeni eklenecek ilaç adı</param>
        /// <param name="allergies">Kullanıcının alerjileri (JSON formatında)</param>
        /// <returns>Hazırlanmış prompt metni</returns>
        string CreateInteractionAnalysisPrompt(
            string existingMedicines,
            string newMedicineName,
            string? allergies = null);
    }
}

