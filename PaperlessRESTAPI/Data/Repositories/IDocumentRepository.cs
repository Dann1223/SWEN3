using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Document-specific repository interface
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> SearchByTextAsync(string searchTerm);
    Task<Document?> GetByFileNameAsync(string fileName);
    Task<IEnumerable<Document>> GetByTagAsync(string tagName);
    Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count = 10);
    Task<IEnumerable<Document>> GetUnprocessedDocumentsAsync();
    Task<IEnumerable<Document>> GetUnindexedDocumentsAsync();
    Task<IEnumerable<Document>> GetDocumentsWithIncludesAsync();
    Task<Document?> GetDocumentWithIncludesAsync(int id);
}
