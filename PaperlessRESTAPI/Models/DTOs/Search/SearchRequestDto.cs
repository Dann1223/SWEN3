namespace PaperlessRESTAPI.Models.DTOs.Search;

/// <summary>
/// Search request DTO
/// </summary>
public class SearchRequestDto
{
    /// <summary>
    /// Search query text
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Tags to filter by
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// File types to filter by
    /// </summary>
    public List<string>? FileTypes { get; set; }

    /// <summary>
    /// Date range filter - start date
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Date range filter - end date
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Only include processed documents
    /// </summary>
    public bool ProcessedOnly { get; set; } = false;

    /// <summary>
    /// Only include AI processed documents
    /// </summary>
    public bool AIProcessedOnly { get; set; } = false;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field
    /// </summary>
    public string SortBy { get; set; } = "uploadDate";

    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";

    /// <summary>
    /// Enable highlighting in results
    /// </summary>
    public bool EnableHighlight { get; set; } = true;

    /// <summary>
    /// Minimum confidence score for results
    /// </summary>
    public float? MinConfidence { get; set; }
}
