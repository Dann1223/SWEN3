using Microsoft.EntityFrameworkCore;
using Nest;
using PaperlessRESTAPI.Data;
using PaperlessRESTAPI.Models.DTOs.Search;
using PaperlessRESTAPI.Models.Search;
using PaperlessRESTAPI.Services.Interfaces;
using System.Diagnostics;

namespace PaperlessRESTAPI.Services.Implementations;

/// <summary>
/// Elasticsearch search service implementation
/// </summary>
public class SearchService : ISearchService
{
    private readonly IElasticClient _elasticClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SearchService> _logger;
    private const string IndexName = "documents";

    public SearchService(
        IElasticClient elasticClient, 
        ApplicationDbContext context, 
        ILogger<SearchService> logger)
    {
        _elasticClient = elasticClient;
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IndexDocumentAsync(DocumentIndex documentIndex)
    {
        try
        {
            var response = await _elasticClient.IndexAsync(documentIndex, i => i
                .Index(IndexName)
                .Id(documentIndex.Id));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index document {DocumentId}: {Error}", 
                    documentIndex.Id, response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully indexed document {DocumentId}", documentIndex.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {DocumentId}", documentIndex.Id);
            return false;
        }
    }

    public async Task<bool> UpdateDocumentAsync(DocumentIndex documentIndex)
    {
        try
        {
            var response = await _elasticClient.UpdateAsync<DocumentIndex>(documentIndex.Id, u => u
                .Index(IndexName)
                .Doc(documentIndex)
                .DocAsUpsert(true));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to update document {DocumentId}: {Error}", 
                    documentIndex.Id, response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully updated document {DocumentId}", documentIndex.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", documentIndex.Id);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        try
        {
            var response = await _elasticClient.DeleteAsync<DocumentIndex>(documentId, d => d
                .Index(IndexName));

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogError("Failed to delete document {DocumentId}: {Error}", 
                    documentId, response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully deleted document {DocumentId}", documentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<SearchResponseDto> SearchDocumentsAsync(SearchRequestDto request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var searchRequest = BuildSearchRequest(request);
            var response = await _elasticClient.SearchAsync<DocumentIndex>(searchRequest);

            if (!response.IsValid)
            {
                _logger.LogError("Search failed: {Error}", response.OriginalException?.Message);
                return new SearchResponseDto();
            }

            var results = new List<SearchResultDto>();
            foreach (var hit in response.Hits)
            {
                var doc = hit.Source;
                _logger.LogInformation("Document {DocId} has score {Score}", doc.Id, hit.Score);
                results.Add(new SearchResultDto
                {
                    Id = doc.Id,
                    Title = doc.Title,
                    FileName = doc.FileName,
                    FileType = doc.FileType,
                    ContentSnippet = GetContentSnippet(doc.Content, request.Query),
                    Summary = doc.Summary,
                    Tags = doc.Tags,
                    UploadDate = doc.UploadDate,
                    LastModified = doc.LastModified,
                    FileSize = doc.FileSize,
                    Score = (float)(hit.Score ?? (1.0 + (doc.Id * 0.1))), // Temporary test score
                    Confidence = doc.Confidence,
                    HasAccess = true,
                    Highlights = GetHighlights(response, doc.Id)
                });
            }

            var totalPages = (int)Math.Ceiling((double)response.Total / request.PageSize);

            return new SearchResponseDto
            {
                Results = results,
                TotalCount = response.Total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                ExecutionTime = stopwatch.ElapsedMilliseconds,
                HasMore = request.Page < totalPages,
                Aggregations = GetAggregations(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing search");
            return new SearchResponseDto
            {
                ExecutionTime = stopwatch.ElapsedMilliseconds
            };
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    public async Task<List<string>> GetSuggestionsAsync(string query, int maxSuggestions = 10)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<DocumentIndex>(s => s
                .Index(IndexName)
                .Size(0)
                .Suggest(sg => sg
                    .Term("title_suggest", t => t
                        .Field(f => f.Title)
                        .Text(query)
                        .Size(maxSuggestions))
                    .Term("content_suggest", t => t
                        .Field(f => f.Content)
                        .Text(query)
                        .Size(maxSuggestions))));

            if (!response.IsValid)
            {
                _logger.LogError("Suggestion request failed: {Error}", response.OriginalException?.Message);
                return new List<string>();
            }

            var suggestions = new HashSet<string>();
            
            foreach (var suggest in response.Suggest.Values)
            {
                foreach (var option in suggest.SelectMany(s => s.Options))
                {
                    suggestions.Add(option.Text);
                }
            }

            return suggestions.Take(maxSuggestions).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions for query: {Query}", query);
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, object>> GetAggregationsAsync(string? query = null)
    {
        try
        {
            var searchRequest = new SearchDescriptor<DocumentIndex>()
                .Index(IndexName)
                .Size(0);

            if (!string.IsNullOrEmpty(query))
            {
                searchRequest = searchRequest.Query(q => BuildQuery(q, query));
            }

            searchRequest = searchRequest.Aggregations(a => a
                .Terms("file_types", t => t.Field(f => f.FileType).Size(20))
                .Terms("tags", t => t.Field(f => f.Tags).Size(50))
                .DateHistogram("upload_dates", d => d
                    .Field(f => f.UploadDate)
                    .CalendarInterval(DateInterval.Month))
                .Range("file_sizes", r => r
                    .Field(f => f.FileSize)
                    .Ranges(
                        rng => rng.To(1024 * 1024), // < 1MB
                        rng => rng.From(1024 * 1024).To(10 * 1024 * 1024), // 1-10MB
                        rng => rng.From(10 * 1024 * 1024) // > 10MB
                    )));

            var response = await _elasticClient.SearchAsync<DocumentIndex>(searchRequest);

            if (!response.IsValid)
            {
                _logger.LogError("Aggregation request failed: {Error}", response.OriginalException?.Message);
                return new Dictionary<string, object>();
            }

            return GetAggregations(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregations");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<SearchResultDto>> GetSimilarDocumentsAsync(int documentId, int maxResults = 10)
    {
        try
        {
            var response = await _elasticClient.SearchAsync<DocumentIndex>(s => s
                .Index(IndexName)
                .Size(maxResults)
                .Query(q => q
                    .MoreLikeThis(mlt => mlt
                        .Like(l => l.Document(d => d.Index(IndexName).Id(documentId)))
                        .Fields(f => f.Field(doc => doc.Title).Field(doc => doc.Content).Field(doc => doc.Summary)))));

            if (!response.IsValid)
            {
                _logger.LogError("Similar documents request failed: {Error}", response.OriginalException?.Message);
                return new List<SearchResultDto>();
            }

            return response.Documents.Select(doc => new SearchResultDto
            {
                Id = doc.Id,
                Title = doc.Title,
                FileName = doc.FileName,
                FileType = doc.FileType,
                Summary = doc.Summary,
                Tags = doc.Tags,
                UploadDate = doc.UploadDate,
                LastModified = doc.LastModified,
                FileSize = doc.FileSize,
                Score = (float)(response.HitsMetadata?.Hits?.FirstOrDefault(h => h.Source.Id == doc.Id)?.Score ?? 0),
                Confidence = doc.Confidence,
                HasAccess = true
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar documents for {DocumentId}", documentId);
            return new List<SearchResultDto>();
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _elasticClient.PingAsync();
            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return false;
        }
    }

    public async Task<bool> CreateIndexAsync()
    {
        try
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName);
            if (indexExists.Exists)
            {
                _logger.LogInformation("Index {IndexName} already exists", IndexName);
                return true;
            }

            var response = await _elasticClient.Indices.CreateAsync(IndexName, c => c
                .Map<DocumentIndex>(m => m.AutoMap()));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to create index {IndexName}: {Error}", 
                    IndexName, response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully created index {IndexName}", IndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", IndexName);
            return false;
        }
    }

    public async Task<bool> DeleteIndexAsync()
    {
        try
        {
            var response = await _elasticClient.Indices.DeleteAsync(IndexName);
            
            if (!response.IsValid && !response.ServerError?.Error?.Type?.Equals("index_not_found_exception", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogError("Failed to delete index {IndexName}: {Error}", 
                    IndexName, response.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Successfully deleted index {IndexName}", IndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", IndexName);
            return false;
        }
    }

    public async Task<int> ReindexAllDocumentsAsync()
    {
        try
        {
            var documents = await _context.Documents
                .Include(d => d.Tags)
                .ToListAsync();

            var documentIndexes = documents.Select(doc => new DocumentIndex
            {
                Id = doc.Id,
                Title = doc.Title,
                FileName = doc.FileName,
                FileType = Path.GetExtension(doc.FileName).TrimStart('.'),
                Content = doc.Content ?? string.Empty,
                Summary = doc.Summary,
                Tags = doc.Tags.Select(t => t.Name).ToList(),
                UploadDate = doc.UploadDate,
                LastModified = doc.LastModified,
                FileSize = doc.FileSize,
                IsProcessed = doc.IsProcessed,
                Confidence = doc.Confidence,
                IsAIProcessed = doc.IsAIProcessed
            }).ToList();

            return await BulkIndexDocumentsAsync(documentIndexes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reindexing all documents");
            return 0;
        }
    }

    public async Task<int> BulkIndexDocumentsAsync(List<DocumentIndex> documents)
    {
        try
        {
            var response = await _elasticClient.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(documents));

            if (!response.IsValid)
            {
                _logger.LogError("Bulk index failed: {Error}", response.OriginalException?.Message);
                return 0;
            }

            var successCount = response.Items.Count(i => i.IsValid);
            _logger.LogInformation("Successfully bulk indexed {SuccessCount}/{TotalCount} documents", 
                successCount, documents.Count);
            
            return successCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk indexing documents");
            return 0;
        }
    }

    #region Private Helper Methods

    private SearchDescriptor<DocumentIndex> BuildSearchRequest(SearchRequestDto request)
    {
        var searchDescriptor = new SearchDescriptor<DocumentIndex>()
            .Index(IndexName)
            .From((request.Page - 1) * request.PageSize)
            .Size(request.PageSize);

        // Build query
        if (!string.IsNullOrEmpty(request.Query))
        {
            searchDescriptor = searchDescriptor.Query(q => BuildQuery(q, request.Query));
        }
        else
        {
            // If no query, use match_all but with a boost to maintain scoring
            searchDescriptor = searchDescriptor.Query(q => q.MatchAll());
        }

        // Add filters
        searchDescriptor = AddFilters(searchDescriptor, request);

        // Add sorting
        searchDescriptor = AddSorting(searchDescriptor, request);

        // Add highlighting
        if (request.EnableHighlight)
        {
            searchDescriptor = searchDescriptor.Highlight(h => h
                .PreTags("<mark>")
                .PostTags("</mark>")
                .Fields(
                    f => f.Field(doc => doc.Title).FragmentSize(150).NumberOfFragments(3),
                    f => f.Field(doc => doc.Content).FragmentSize(150).NumberOfFragments(3),
                    f => f.Field(doc => doc.Summary).FragmentSize(150).NumberOfFragments(1)
                ));
        }

        return searchDescriptor;
    }

    private QueryContainer BuildQuery(QueryContainerDescriptor<DocumentIndex> q, string query)
    {
        return q.Bool(b => b
            .Should(
                s => s.Match(m => m.Field(f => f.Title).Query(query).Boost(3)),
                s => s.Match(m => m.Field(f => f.Summary).Query(query).Boost(2)),
                s => s.Match(m => m.Field(f => f.Content).Query(query)),
                s => s.Match(m => m.Field(f => f.Tags).Query(query).Boost(2))
            )
            .MinimumShouldMatch(1));
    }

    private SearchDescriptor<DocumentIndex> AddFilters(SearchDescriptor<DocumentIndex> searchDescriptor, SearchRequestDto request)
    {
        var filters = new List<Func<QueryContainerDescriptor<DocumentIndex>, QueryContainer>>();

        if (request.Tags?.Any() == true)
        {
            filters.Add(f => f.Terms(t => t.Field(doc => doc.Tags).Terms(request.Tags)));
        }

        if (request.FileTypes?.Any() == true)
        {
            filters.Add(f => f.Terms(t => t.Field(doc => doc.FileType).Terms(request.FileTypes)));
        }

        if (request.DateFrom.HasValue || request.DateTo.HasValue)
        {
            filters.Add(f => f.DateRange(dr => dr
                .Field(doc => doc.UploadDate)
                .GreaterThanOrEquals(request.DateFrom)
                .LessThanOrEquals(request.DateTo)));
        }

        if (request.ProcessedOnly)
        {
            filters.Add(f => f.Term(t => t.Field(doc => doc.IsProcessed).Value(true)));
        }

        if (request.AIProcessedOnly)
        {
            filters.Add(f => f.Term(t => t.Field(doc => doc.IsAIProcessed).Value(true)));
        }

        if (request.MinConfidence.HasValue)
        {
            filters.Add(f => f.Range(r => r.Field(doc => doc.Confidence).GreaterThanOrEquals(request.MinConfidence)));
        }

        if (filters.Any())
        {
            searchDescriptor = searchDescriptor.PostFilter(pf => pf.Bool(b => b.Must(filters.ToArray())));
        }

        return searchDescriptor;
    }

    private SearchDescriptor<DocumentIndex> AddSorting(SearchDescriptor<DocumentIndex> searchDescriptor, SearchRequestDto request)
    {
        var sortOrder = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) 
            ? SortOrder.Ascending 
            : SortOrder.Descending;

        return request.SortBy.ToLower() switch
        {
            "title" => searchDescriptor.Sort(s => s.Field(f => f.Title.Suffix("keyword"), sortOrder)),
            "filename" => searchDescriptor.Sort(s => s.Field(f => f.FileName.Suffix("keyword"), sortOrder)),
            "filesize" => searchDescriptor.Sort(s => s.Field(f => f.FileSize, sortOrder)),
            "confidence" => searchDescriptor.Sort(s => s.Field(f => f.Confidence, sortOrder)),
            "lastmodified" => searchDescriptor.Sort(s => s.Field(f => f.LastModified, sortOrder)),
            _ => searchDescriptor.Sort(s => s.Field(f => f.UploadDate, sortOrder))
        };
    }

    private string GetContentSnippet(string content, string query)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(query))
            return content?.Substring(0, Math.Min(content.Length, 200)) + "..." ?? string.Empty;

        var queryIndex = content.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (queryIndex == -1)
            return content.Substring(0, Math.Min(content.Length, 200)) + "...";

        var start = Math.Max(0, queryIndex - 100);
        var length = Math.Min(300, content.Length - start);
        
        return (start > 0 ? "..." : "") + 
               content.Substring(start, length) + 
               (start + length < content.Length ? "..." : "");
    }

    private Dictionary<string, List<string>>? GetHighlights(ISearchResponse<DocumentIndex> response, int documentId)
    {
        var hit = response.HitsMetadata?.Hits?.FirstOrDefault(h => h.Source.Id == documentId);
        if (hit?.Highlight == null)
            return null;

        return hit.Highlight.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToList());
    }

    private Dictionary<string, object> GetAggregations(ISearchResponse<DocumentIndex> response)
    {
        var aggregations = new Dictionary<string, object>();

        if (response.Aggregations == null)
            return aggregations;

        foreach (var agg in response.Aggregations)
        {
            aggregations[agg.Key] = ExtractAggregationValue(agg.Value);
        }

        return aggregations;
    }

    private object ExtractAggregationValue(IAggregate aggregate)
    {
        return aggregate switch
        {
            BucketAggregate bucketAgg => bucketAgg.Items.OfType<KeyedBucket<object>>()
                .ToDictionary(b => b.Key.ToString(), b => b.DocCount),
            ValueAggregate valueAgg => valueAgg.Value,
            _ => aggregate.ToString()
        };
    }

    #endregion

    public async Task<int> SyncDocumentIndexingStatusAsync()
    {
        try
        {
            _logger.LogInformation("Starting to sync document indexing status");

            // Get all documents from Elasticsearch
            var searchResponse = await _elasticClient.SearchAsync<DocumentIndex>(s => s
                .Index(IndexName)
                .Size(1000)
                .Query(q => q.MatchAll()));

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Failed to get documents from Elasticsearch: {Error}", 
                    searchResponse.OriginalException?.Message);
                return 0;
            }

            var indexedDocumentIds = searchResponse.Documents.Select(d => d.Id).ToHashSet();
            _logger.LogInformation("Found {Count} documents in Elasticsearch", indexedDocumentIds.Count);

            // Update database records
            var documents = await _context.Documents
                .Where(d => indexedDocumentIds.Contains(d.Id) && !d.IsIndexed)
                .ToListAsync();

            var syncedCount = 0;
            foreach (var document in documents)
            {
                document.IsIndexed = true;
                syncedCount++;
            }

            if (syncedCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated indexing status for {Count} documents", syncedCount);
            }

            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing document indexing status");
            return 0;
        }
    }
}
