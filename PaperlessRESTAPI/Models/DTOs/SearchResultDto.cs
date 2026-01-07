namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Document search result DTO containing documents and metadata
/// </summary>
public class DocumentSearchResultDto
{
    public List<DocumentDto> Documents { get; set; } = new();
    public int TotalCount { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public TimeSpan SearchDuration { get; set; }
}
