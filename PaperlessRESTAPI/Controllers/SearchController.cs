using Microsoft.AspNetCore.Mvc;
using PaperlessRESTAPI.Models.DTOs.Search;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Controllers;

/// <summary>
/// Search controller for document search operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Search documents with advanced filtering
    /// </summary>
    /// <param name="request">Search parameters</param>
    /// <returns>Search results</returns>
    [HttpPost("documents")]
    public async Task<ActionResult<SearchResponseDto>> SearchDocuments([FromBody] SearchRequestDto request)
    {
        try
        {
            // Validate request
            if (request.Page < 1)
                request.Page = 1;
            
            if (request.PageSize < 1 || request.PageSize > 100)
                request.PageSize = 20;

            var results = await _searchService.SearchDocumentsAsync(request);
            
            _logger.LogInformation("Search completed for query '{Query}' with {ResultCount} results", 
                request.Query, results.TotalCount);

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search for query: {Query}", request.Query);
            return StatusCode(500, "An error occurred while searching documents");
        }
    }

    /// <summary>
    /// Get search suggestions for autocomplete
    /// </summary>
    /// <param name="query">Partial search query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions (default: 10)</param>
    /// <returns>List of suggested terms</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetSuggestions(
        [FromQuery] string query, 
        [FromQuery] int maxSuggestions = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required");

            if (maxSuggestions < 1 || maxSuggestions > 50)
                maxSuggestions = 10;

            var suggestions = await _searchService.GetSuggestionsAsync(query, maxSuggestions);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for query: {Query}", query);
            return StatusCode(500, "An error occurred while getting suggestions");
        }
    }

    /// <summary>
    /// Get search aggregations/facets
    /// </summary>
    /// <param name="query">Optional base query for filtered aggregations</param>
    /// <returns>Aggregation data</returns>
    [HttpGet("aggregations")]
    public async Task<ActionResult<Dictionary<string, object>>> GetAggregations([FromQuery] string? query = null)
    {
        try
        {
            var aggregations = await _searchService.GetAggregationsAsync(query);
            return Ok(aggregations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregations for query: {Query}", query);
            return StatusCode(500, "An error occurred while getting aggregations");
        }
    }

    /// <summary>
    /// Find documents similar to a given document
    /// </summary>
    /// <param name="documentId">Reference document ID</param>
    /// <param name="maxResults">Maximum number of similar documents (default: 10)</param>
    /// <returns>List of similar documents</returns>
    [HttpGet("similar/{documentId:int}")]
    public async Task<ActionResult<List<SearchResultDto>>> GetSimilarDocuments(
        int documentId, 
        [FromQuery] int maxResults = 10)
    {
        try
        {
            if (maxResults < 1 || maxResults > 50)
                maxResults = 10;

            var similarDocuments = await _searchService.GetSimilarDocumentsAsync(documentId, maxResults);
            return Ok(similarDocuments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar documents for document: {DocumentId}", documentId);
            return StatusCode(500, "An error occurred while finding similar documents");
        }
    }

    /// <summary>
    /// Check search service health
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public async Task<ActionResult<object>> GetHealth()
    {
        try
        {
            var isHealthy = await _searchService.IsHealthyAsync();
            
            var status = new
            {
                Status = isHealthy ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Service = "Elasticsearch"
            };

            return isHealthy ? Ok(status) : StatusCode(503, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking search service health");
            return StatusCode(500, new
            {
                Status = "Error",
                Timestamp = DateTime.UtcNow,
                Service = "Elasticsearch",
                Error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Create or recreate the search index
    /// </summary>
    /// <returns>Operation result</returns>
    [HttpPost("index/create")]
    public async Task<ActionResult<object>> CreateIndex()
    {
        try
        {
            var success = await _searchService.CreateIndexAsync();
            
            var result = new
            {
                Success = success,
                Message = success ? "Index created successfully" : "Failed to create index",
                Timestamp = DateTime.UtcNow
            };

            return success ? Ok(result) : StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating search index");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while creating the index",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Delete the search index
    /// </summary>
    /// <returns>Operation result</returns>
    [HttpDelete("index")]
    public async Task<ActionResult<object>> DeleteIndex()
    {
        try
        {
            var success = await _searchService.DeleteIndexAsync();
            
            var result = new
            {
                Success = success,
                Message = success ? "Index deleted successfully" : "Failed to delete index",
                Timestamp = DateTime.UtcNow
            };

            return success ? Ok(result) : StatusCode(500, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting search index");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while deleting the index",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Reindex all documents from the database
    /// </summary>
    /// <returns>Number of documents reindexed</returns>
    [HttpPost("index/rebuild")]
    public async Task<ActionResult<object>> RebuildIndex()
    {
        try
        {
            _logger.LogInformation("Starting index rebuild...");
            
            // Delete existing index
            await _searchService.DeleteIndexAsync();
            
            // Create new index
            var indexCreated = await _searchService.CreateIndexAsync();
            if (!indexCreated)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Failed to create new index",
                    Timestamp = DateTime.UtcNow
                });
            }

            // Reindex all documents
            var reindexedCount = await _searchService.ReindexAllDocumentsAsync();
            
            _logger.LogInformation("Index rebuild completed. Reindexed {Count} documents", reindexedCount);
            
            return Ok(new
            {
                Success = true,
                Message = $"Successfully reindexed {reindexedCount} documents",
                Count = reindexedCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rebuilding search index");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while rebuilding the index",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Synchronize document indexing status between database and Elasticsearch
    /// </summary>
    [HttpPost("sync-status")]
    public async Task<IActionResult> SyncIndexingStatus()
    {
        try
        {
            _logger.LogInformation("Starting to sync document indexing status");
            
            var syncedCount = await _searchService.SyncDocumentIndexingStatusAsync();
            
            _logger.LogInformation("Synced indexing status for {Count} documents", syncedCount);
            
            return Ok(new
            {
                Success = true,
                Message = $"Successfully synced indexing status for {syncedCount} documents",
                Count = syncedCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing document indexing status");
            return StatusCode(500, new
            {
                Success = false,
                Message = "An error occurred while syncing indexing status",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
