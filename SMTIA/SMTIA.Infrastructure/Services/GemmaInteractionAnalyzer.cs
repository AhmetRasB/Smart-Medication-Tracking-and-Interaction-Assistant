using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMTIA.Application.Services;
using SMTIA.Infrastructure.Options;

namespace SMTIA.Infrastructure.Services
{
    internal sealed class GemmaInteractionAnalyzer : IGemmaInteractionAnalyzer
    {
        private readonly HttpClient _httpClient;
        private readonly GemmaOptions _options;
        private readonly ILogger<GemmaInteractionAnalyzer> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public GemmaInteractionAnalyzer(
            HttpClient httpClient, 
            IOptions<GemmaOptions> options,
            ILogger<GemmaInteractionAnalyzer> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Groq API için authorization header ekle
            if (!string.IsNullOrWhiteSpace(_options.ApiToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiToken);
            }

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SMTIA-API/1.0");
        }

        public async Task<string> AnalyzeAsync(string prompt, CancellationToken cancellationToken = default)
        {
            try
            {
                // Groq API formatı (OpenAI uyumlu)
                var requestBody = new
                {
                    model = _options.ModelName,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _options.MaxTokens,
                    temperature = _options.Temperature
                };

                _logger.LogInformation("Groq API'ye istek gönderiliyor. Model: {Model}", _options.ModelName);
                _logger.LogDebug("Prompt: {Prompt}", prompt);

                var response = await _httpClient.PostAsJsonAsync(_options.BaseUrl, requestBody, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Groq API hatası: {StatusCode}. Yanıt: {ErrorContent}", response.StatusCode, errorContent);
                    throw new HttpRequestException(
                        $"Groq API hatası: {response.StatusCode}. Yanıt: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Groq API yanıtı: {Response}", responseContent);

                // Groq API yanıt formatı: { "choices": [{ "message": { "content": "..." } }] }
                var jsonDoc = JsonDocument.Parse(responseContent);
                
                if (jsonDoc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var content))
                    {
                        var result = content.GetString() ?? string.Empty;
                        _logger.LogInformation("Groq API'den başarıyla yanıt alındı. Uzunluk: {Length}", result.Length);
                        return result;
                    }
                }

                // Eğer beklenen formatta değilse, ham yanıtı döndür
                _logger.LogWarning("Groq API yanıtı beklenen formatta değil. Ham yanıt döndürülüyor.");
                return responseContent;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parse hatası");
                throw new InvalidOperationException(
                    $"Groq API yanıtı ayrıştırılamadı: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq analizi sırasında beklenmeyen hata");
                throw new InvalidOperationException(
                    $"Groq analizi sırasında bir hata oluştu: {ex.Message}", ex);
            }
        }
    }
}

