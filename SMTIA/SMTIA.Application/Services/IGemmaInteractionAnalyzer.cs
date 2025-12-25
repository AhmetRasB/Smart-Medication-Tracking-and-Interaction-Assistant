namespace SMTIA.Application.Services
{
    /// <summary>
    /// Gemma AI modeli ile ilaç etkileşim analizi yapan servis
    /// </summary>
    public interface IGemmaInteractionAnalyzer
    {
        /// <summary>
        /// Gemma modeline prompt gönderir ve yanıtı alır
        /// </summary>
        /// <param name="prompt">AI'ya gönderilecek prompt metni</param>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>AI'dan gelen ham yanıt metni</returns>
        Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default);
    }
}

