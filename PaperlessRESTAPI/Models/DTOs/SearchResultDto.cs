namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Search result DTO containing documents and metadata
/// </summary>
public class SearchResultDto
{
    public List<DocumentDto> Documents { get; set; } = new();
    public int TotalCount { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public TimeSpan SearchDuration { get; set; }
}
