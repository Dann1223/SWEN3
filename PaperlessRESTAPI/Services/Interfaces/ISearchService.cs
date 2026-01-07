using PaperlessRESTAPI.Models.DTOs.Search;
using PaperlessRESTAPI.Models.Search;

namespace PaperlessRESTAPI.Services.Interfaces;

/// <summary>
/// Search service interface for Elasticsearch operations
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Index a document in Elasticsearch
    /// </summary>
    /// <param name="documentIndex">Document to index</param>
    /// <returns>Success status</returns>
    Task<bool> IndexDocumentAsync(DocumentIndex documentIndex);

    /// <summary>
    /// Update a document in the search index
    /// </summary>
    /// <param name="documentIndex">Updated document</param>
    /// <returns>Success status</returns>
    Task<bool> UpdateDocumentAsync(DocumentIndex documentIndex);

    /// <summary>
    /// Remove a document from the search index
    /// </summary>
    /// <param name="documentId">Document ID to remove</param>
    /// <returns>Success status</returns>
    Task<bool> DeleteDocumentAsync(int documentId);

    /// <summary>
    /// Search documents with advanced filters
    /// </summary>
    /// <param name="request">Search request parameters</param>
    /// <returns>Search results</returns>
    Task<SearchResponseDto> SearchDocumentsAsync(SearchRequestDto request);

    /// <summary>
    /// Get suggestions for search autocomplete
    /// </summary>
    /// <param name="query">Partial query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <returns>Suggested terms</returns>
    Task<List<string>> GetSuggestionsAsync(string query, int maxSuggestions = 10);

    /// <summary>
    /// Get search aggregations/facets
    /// </summary>
    /// <param name="query">Optional base query</param>
    /// <returns>Aggregation results</returns>
    Task<Dictionary<string, object>> GetAggregationsAsync(string? query = null);

    /// <summary>
    /// Perform "More Like This" search
    /// </summary>
    /// <param name="documentId">Reference document ID</param>
    /// <param name="maxResults">Maximum results to return</param>
    /// <returns>Similar documents</returns>
    Task<List<SearchResultDto>> GetSimilarDocumentsAsync(int documentId, int maxResults = 10);

    /// <summary>
    /// Check if search service is healthy
    /// </summary>
    /// <returns>Health status</returns>
    Task<bool> IsHealthyAsync();

    /// <summary>
    /// Create or update the document index mapping
    /// </summary>
    /// <returns>Success status</returns>
    Task<bool> CreateIndexAsync();

    /// <summary>
    /// Delete the entire search index
    /// </summary>
    /// <returns>Success status</returns>
    Task<bool> DeleteIndexAsync();

    /// <summary>
    /// Reindex all documents from database
    /// </summary>
    /// <returns>Number of documents reindexed</returns>
    Task<int> ReindexAllDocumentsAsync();

    /// <summary>
    /// Bulk index multiple documents
    /// </summary>
    /// <param name="documents">Documents to index</param>
    /// <returns>Number of successfully indexed documents</returns>
    Task<int> BulkIndexDocumentsAsync(List<DocumentIndex> documents);

    /// <summary>
    /// Synchronize document indexing status between database and Elasticsearch
    /// </summary>
    /// <returns>Number of documents synced</returns>
    Task<int> SyncDocumentIndexingStatusAsync();
}
