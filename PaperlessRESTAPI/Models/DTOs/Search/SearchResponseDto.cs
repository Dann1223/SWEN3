namespace PaperlessRESTAPI.Models.DTOs.Search;

/// <summary>
/// Search response DTO
/// </summary>
public class SearchResponseDto
{
    /// <summary>
    /// Search results
    /// </summary>
    public List<SearchResultDto> Results { get; set; } = new List<SearchResultDto>();

    /// <summary>
    /// Total number of results
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Search execution time in milliseconds
    /// </summary>
    public long ExecutionTime { get; set; }

    /// <summary>
    /// Whether there are more results
    /// </summary>
    public bool HasMore { get; set; }

    /// <summary>
    /// Search aggregations/facets
    /// </summary>
    public Dictionary<string, object>? Aggregations { get; set; }
}

/// <summary>
/// Individual search result
/// </summary>
public class SearchResultDto
{
    /// <summary>
    /// Document ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Document title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File type
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Content snippet with highlights
    /// </summary>
    public string? ContentSnippet { get; set; }

    /// <summary>
    /// AI-generated summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Associated tags
    /// </summary>
    public List<string> Tags { get; set; } = new List<string>();

    /// <summary>
    /// Upload date
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Last modified date
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Search relevance score
    /// </summary>
    public float Score { get; set; }

    /// <summary>
    /// Processing confidence
    /// </summary>
    public float? Confidence { get; set; }

    /// <summary>
    /// Whether user has permission to access this document
    /// </summary>
    public bool HasAccess { get; set; } = true;

    /// <summary>
    /// User's permission level for this document
    /// </summary>
    public string? PermissionLevel { get; set; }

    /// <summary>
    /// Highlighted fields
    /// </summary>
    public Dictionary<string, List<string>>? Highlights { get; set; }
}
