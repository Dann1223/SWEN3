using PaperlessServices.Services.Interfaces;
using PaperlessServices.Models;
using System.Text;
using System.Text.Json;

namespace PaperlessServices.Services.Implementations;

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClientService> _logger;
    private readonly IConfiguration _configuration;

    public ApiClientService(
        HttpClient httpClient, 
        ILogger<ApiClientService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<bool> SendOcrResultAsync(OcrResult ocrResult)
    {
        try
        {
            var apiBaseUrl = _configuration.GetValue<string>("ApiClient:BaseUrl") ?? "http://paperless-api:8080";
            var endpoint = $"{apiBaseUrl}/api/documents/{ocrResult.DocumentId}/ocr-result";

            var ocrResultDto = new
            {
                DocumentId = ocrResult.DocumentId,
                CorrelationId = ocrResult.CorrelationId,
                Success = ocrResult.Success,
                ExtractedText = ocrResult.ExtractedText,
                Confidence = ocrResult.Confidence,
                ErrorMessage = ocrResult.ErrorMessage,
                ProcessedAt = ocrResult.ProcessedAt
            };

            var json = JsonSerializer.Serialize(ocrResultDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending OCR result to API: {Endpoint} for document {DocumentId}", 
                endpoint, ocrResult.DocumentId);

            var response = await _httpClient.PostAsync(endpoint, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully sent OCR result to API for document {DocumentId}", 
                    ocrResult.DocumentId);
                return true;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send OCR result to API for document {DocumentId}. " +
                    "Status: {StatusCode}, Response: {Response}", 
                    ocrResult.DocumentId, response.StatusCode, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OCR result to API for document {DocumentId}", 
                ocrResult.DocumentId);
            return false;
        }
    }
}
