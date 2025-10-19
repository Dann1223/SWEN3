namespace PaperlessRESTAPI.Models.Messages;

public class OcrMessage
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public class GenAIMessage
{
    public int DocumentId { get; set; }
    public string OcrText { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public class IndexingMessage
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OcrText { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

public class ProcessingResultMessage
{
    public int DocumentId { get; set; }
    public string ProcessingType { get; set; } = string.Empty; // OCR, GenAI, Indexing
    public bool IsSuccess { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
