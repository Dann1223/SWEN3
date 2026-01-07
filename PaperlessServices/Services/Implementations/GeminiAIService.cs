using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaperlessServices.Services.Interfaces;

namespace PaperlessServices.Services.Implementations
{
    public class GeminiAIService : IGenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public GeminiAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["GenAI:ApiKey"] ?? throw new ArgumentException("GenAI:ApiKey not configured");
            _baseUrl = configuration["GenAI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
            
            _httpClient.DefaultRequestHeaders.Add("X-goog-api-key", _apiKey);
        }

        public async Task<string> GenerateSummaryAsync(string text, int maxLength = 200)
        {
            try
            {
                _logger.LogInformation("Starting summary generation for text of {Length} characters", text.Length);

                if (string.IsNullOrWhiteSpace(text))
                {
                    return "No content available for summary.";
                }

                // Truncate very long text to avoid API limits
                var truncatedText = text.Length > 10000 ? text.Substring(0, 10000) + "..." : text;

                var prompt = $@"Please provide a concise summary of the following document in {maxLength} words or less. 
Focus on the main topics, key points, and important information:

{truncatedText}

Summary:";

                var response = await CallGeminiAPIAsync(prompt);
                
                if (string.IsNullOrWhiteSpace(response))
                {
                    return "Unable to generate summary at this time.";
                }

                _logger.LogInformation("Successfully generated summary of {Length} characters", response.Length);
                return response.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate summary for text");
                return "Summary generation failed. Please try again later.";
            }
        }

        public async Task<List<string>> ExtractTagsAsync(string text)
        {
            try
            {
                _logger.LogInformation("Starting tag extraction for text of {Length} characters", text.Length);

                if (string.IsNullOrWhiteSpace(text))
                {
                    return new List<string>();
                }

                // Truncate very long text to avoid API limits
                var truncatedText = text.Length > 5000 ? text.Substring(0, 5000) + "..." : text;

                var prompt = $@"Analyze the following document and extract 5-10 relevant tags or keywords that best describe the content. 
Return only the tags separated by commas, no additional text:

{truncatedText}

Tags:";

                var response = await CallGeminiAPIAsync(prompt);
                
                if (string.IsNullOrWhiteSpace(response))
                {
                    return new List<string>();
                }

                // Parse tags from response
                var tags = response.Split(',', ';', '\n')
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrWhiteSpace(tag) && tag.Length > 2)
                    .Take(10)
                    .ToList();

                _logger.LogInformation("Successfully extracted {Count} tags", tags.Count);
                return tags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract tags from text");
                return new List<string>();
            }
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                var testResponse = await CallGeminiAPIAsync("Hello");
                return !string.IsNullOrEmpty(testResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GenAI service is not available");
                return false;
            }
        }

        private async Task<string> CallGeminiAPIAsync(string prompt)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.7,
                            topK = 40,
                            topP = 0.95,
                            maxOutputTokens = 1024
                        },
                        safetySettings = new[]
                        {
                            new
                            {
                                category = "HARM_CATEGORY_HARASSMENT",
                                threshold = "BLOCK_MEDIUM_AND_ABOVE"
                            },
                            new
                            {
                                category = "HARM_CATEGORY_HATE_SPEECH",
                                threshold = "BLOCK_MEDIUM_AND_ABOVE"
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var url = $"{_baseUrl}/models/gemini-2.0-flash:generateContent";
                    _logger.LogDebug("Calling Gemini API at {Url}, attempt {Attempt}", url, attempt);

                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        return ParseGeminiResponse(responseJson);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Gemini API returned {StatusCode}: {Error} (Attempt {Attempt})", 
                            response.StatusCode, errorContent, attempt);

                        // If it's a rate limit error, wait longer before retry
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            await Task.Delay(RetryDelayMs * attempt * 2);
                        }
                        else if (attempt == MaxRetries)
                        {
                            throw new HttpRequestException($"Gemini API failed with status {response.StatusCode}: {errorContent}");
                        }
                    }
                }
                catch (Exception ex) when (attempt < MaxRetries)
                {
                    _logger.LogWarning(ex, "Gemini API call failed (Attempt {Attempt}), retrying...", attempt);
                    await Task.Delay(RetryDelayMs * attempt);
                }
            }

            throw new Exception("Failed to call Gemini API after all retry attempts");
        }

        private string ParseGeminiResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString() ?? string.Empty;
                        }
                    }
                }

                _logger.LogWarning("Unexpected Gemini API response format: {Response}", jsonResponse);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini API response: {Response}", jsonResponse);
                return string.Empty;
            }
        }
    }
}
