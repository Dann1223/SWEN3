namespace PaperlessServices.Models;

public class OcrMessage
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class OcrResult
{
    public int DocumentId { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
