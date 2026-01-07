namespace PaperlessRESTAPI.Models.DTOs;

public class OcrResultDto
{
    public int DocumentId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}
