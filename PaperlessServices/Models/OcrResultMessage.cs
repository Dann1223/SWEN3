namespace PaperlessServices.Models;

/// <summary>
/// OCR result message for updating document with processed text
/// </summary>
public class OcrResultMessage
{
    public int DocumentId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public float Confidence { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string ProcessingMethod { get; set; } = string.Empty;
}
