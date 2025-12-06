using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SMTIA.Application.Services;

namespace SMTIA.Infrastructure.Services
{
    internal sealed class FdaService : IFdaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FdaService> _logger;
        private const string BaseUrl = "https://api.fda.gov/drug/label.json";

        public FdaService(HttpClient httpClient, ILogger<FdaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FdaSearchResponse> SearchMedicinesAsync(string searchTerm, int limit = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                var normalizedTerm = searchTerm.Trim();
                var searchFilter = $"openfda.brand_name:\"{normalizedTerm}\"+OR+openfda.generic_name:\"{normalizedTerm}\"";
                var encodedFilter = Uri.EscapeDataString(searchFilter);
                var url = $"{BaseUrl}?search={encodedFilter}&limit={limit}";

                _logger.LogInformation("Searching FDA API for: {SearchTerm}", searchTerm);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new FdaSearchResponse(new List<FdaDrugLabel>(), 0);
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("FDA search failed ({StatusCode}): {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"FDA API arama isteği başarısız: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<FdaApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Results == null)
                {
                    return new FdaSearchResponse(new List<FdaDrugLabel>(), 0);
                }

                var drugLabels = result.Results.Select(MapToDrugLabel).ToList();

                return new FdaSearchResponse(drugLabels, result.Meta?.Total ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching FDA API for: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<FdaDrugLabel?> GetDrugLabelByIdAsync(string labelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var filter = $"id:\"{labelId}\"";
                var url = $"{BaseUrl}?search={Uri.EscapeDataString(filter)}&limit=1";

                _logger.LogInformation("Getting FDA drug label by ID: {LabelId}", labelId);

                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("FDA label fetch failed ({StatusCode}): {Error}", response.StatusCode, error);
                    throw new HttpRequestException($"FDA API ilaç detay isteği başarısız: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<FdaApiResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Results == null || !result.Results.Any())
                {
                    return null;
                }

                return MapToDrugLabel(result.Results.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FDA drug label by ID: {LabelId}", labelId);
                throw;
            }
        }

        private static FdaDrugLabel MapToDrugLabel(FdaApiResult apiResult)
        {
            var openFda = apiResult.OpenFda ?? new Dictionary<string, object>();

            return new FdaDrugLabel(
                Id: apiResult.Id ?? string.Empty,
                BrandName: GetFirstValue(openFda, "brand_name"),
                GenericName: GetFirstValue(openFda, "generic_name"),
                ProductType: GetFirstValue(openFda, "product_type"),
                Route: GetFirstValue(openFda, "route"),
                DosageForms: apiResult.DosageFormsAndStrengths?.SelectMany(d => ExtractDosageForms(d)).Distinct().ToList(),
                ActiveIngredients: apiResult.ActiveIngredient,
                Manufacturer: GetFirstValue(openFda, "manufacturer_name"),
                Description: apiResult.Description?.FirstOrDefault(),
                DosageAndAdministration: apiResult.DosageAndAdministration,
                IndicationsAndUsage: apiResult.IndicationsAndUsage,
                BoxedWarning: apiResult.BoxedWarning,
                Warnings: apiResult.Warnings,
                Purpose: apiResult.Purpose,
                AdverseReactions: apiResult.AdverseReactions,
                OpenFda: openFda);
        }

        private static string? GetFirstValue(Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var value))
                return null;

            if (value is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Array)
                {
                    return jsonElement.EnumerateArray().FirstOrDefault().GetString();
                }
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    return jsonElement.GetString();
                }
            }

            return value?.ToString();
        }

        private static List<string> ExtractDosageForms(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var forms = new List<string>();
            var commonForms = new[] { "tablet", "capsule", "syrup", "suspension", "injection", "cream", "gel", "ointment", "drops", "spray" };

            foreach (var form in commonForms)
            {
                if (text.Contains(form, StringComparison.OrdinalIgnoreCase))
                {
                    forms.Add(form);
                }
            }

            return forms;
        }

        private sealed class FdaApiResponse
        {
            public FdaMeta? Meta { get; set; }
            public List<FdaApiResult>? Results { get; set; }
        }

        private sealed class FdaMeta
        {
            public int Total { get; set; }
        }

        private sealed class FdaApiResult
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("openfda")]
            public Dictionary<string, object>? OpenFda { get; set; }

            [JsonPropertyName("dosage_forms_and_strengths")]
            public List<string>? DosageFormsAndStrengths { get; set; }

            [JsonPropertyName("active_ingredient")]
            public List<string>? ActiveIngredient { get; set; }

            [JsonPropertyName("description")]
            public List<string>? Description { get; set; }

            [JsonPropertyName("dosage_and_administration")]
            public List<string>? DosageAndAdministration { get; set; }

            [JsonPropertyName("indications_and_usage")]
            public List<string>? IndicationsAndUsage { get; set; }

            [JsonPropertyName("boxed_warning")]
            public List<string>? BoxedWarning { get; set; }

            [JsonPropertyName("warnings")]
            public List<string>? Warnings { get; set; }

            [JsonPropertyName("purpose")]
            public List<string>? Purpose { get; set; }

            [JsonPropertyName("adverse_reactions")]
            public List<string>? AdverseReactions { get; set; }
        }
    }
}

