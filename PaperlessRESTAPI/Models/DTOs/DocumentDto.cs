namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Document Data Transfer Object for API responses
/// </summary>
public class DocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public DateTime? LastModified { get; set; }
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? OcrText { get; set; }
    public string? Summary { get; set; }
    public bool IsProcessed { get; set; }
    public bool IsIndexed { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}
