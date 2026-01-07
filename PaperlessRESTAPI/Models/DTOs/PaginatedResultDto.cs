namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Paginated result wrapper for API responses
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PaginatedResultDto<T>
{
    /// <summary>
    /// Collection of items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there are more pages available
    /// </summary>
    public bool HasMore => Page < TotalPages;

    /// <summary>
    /// Whether this is the first page
    /// </summary>
    public bool IsFirstPage => Page == 1;

    /// <summary>
    /// Whether this is the last page
    /// </summary>
    public bool IsLastPage => Page == TotalPages;
}
