using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Document repository implementation with specialized queries
/// </summary>
public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> SearchByTextAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var lowerSearchTerm = searchTerm.ToLower();

        return await _dbSet
            .Include(d => d.Tags)
            .Where(d => 
                d.Title.ToLower().Contains(lowerSearchTerm) ||
                d.FileName.ToLower().Contains(lowerSearchTerm) ||
                (d.OcrText != null && d.OcrText.ToLower().Contains(lowerSearchTerm)) ||
                (d.Summary != null && d.Summary.ToLower().Contains(lowerSearchTerm)) ||
                d.Tags.Any(t => t.Name.ToLower().Contains(lowerSearchTerm)))
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<Document?> GetByFileNameAsync(string fileName)
    {
        return await _dbSet
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.FileName == fileName);
    }

    public async Task<IEnumerable<Document>> GetByTagAsync(string tagName)
    {
        return await _dbSet
            .Include(d => d.Tags)
            .Where(d => d.Tags.Any(t => t.Name.ToLower() == tagName.ToLower()))
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetRecentDocumentsAsync(int count = 10)
    {
        return await _dbSet
            .Include(d => d.Tags)
            .OrderByDescending(d => d.UploadDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetUnprocessedDocumentsAsync()
    {
        return await _dbSet
            .Where(d => !d.IsProcessed)
            .OrderBy(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetUnindexedDocumentsAsync()
    {
        return await _dbSet
            .Where(d => d.IsProcessed && !d.IsIndexed)
            .OrderBy(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsWithIncludesAsync()
    {
        return await _dbSet
            .Include(d => d.Tags)
            .Include(d => d.AccessLogs)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public async Task<Document?> GetDocumentWithIncludesAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Tags)
            .Include(d => d.AccessLogs)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public override async Task<IEnumerable<Document>> GetAllAsync()
    {
        return await _dbSet
            .Include(d => d.Tags)
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync();
    }

    public override async Task<Document?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}
