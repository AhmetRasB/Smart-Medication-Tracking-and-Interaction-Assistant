namespace SMTIA.Infrastructure.Options
{
    public sealed class GemmaOptions
    {
        /// <summary>
        /// Groq API key (ücretsiz: https://console.groq.com/)
        /// </summary>
        public string ApiToken { get; set; } = string.Empty;

        /// <summary>
        /// Model adı (Groq'da kullanılabilir modeller: llama-3.1-8b-instant, mixtral-8x7b-32768, gemma2-9b-it, vb.)
        /// </summary>
        public string ModelName { get; set; } = "llama-3.1-8b-instant";

        /// <summary>
        /// Groq API base URL
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/chat/completions";

        /// <summary>
        /// Maksimum token sayısı
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Temperature (0.0 - 2.0)
        /// </summary>
        public double Temperature { get; set; } = 0.7;
    }
}

