using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Repository implementation for daily document access statistics
/// </summary>
public class DailyDocumentAccessRepository : Repository<DailyDocumentAccess>, IDailyDocumentAccessRepository
{
    public DailyDocumentAccessRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DailyDocumentAccess?> GetByDocumentAndDateAsync(int documentId, DateOnly date)
    {
        return await _context.DailyDocumentAccesses
            .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.AccessDate == date);
    }

    public async Task<IEnumerable<DailyDocumentAccess>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.DailyDocumentAccesses
            .Where(d => d.AccessDate >= startDate && d.AccessDate <= endDate)
            .Include(d => d.Document)
            .OrderBy(d => d.AccessDate)
            .ThenBy(d => d.DocumentId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyDocumentAccess>> GetByDocumentIdAsync(int documentId)
    {
        return await _context.DailyDocumentAccesses
            .Where(d => d.DocumentId == documentId)
            .OrderBy(d => d.AccessDate)
            .ToListAsync();
    }

    public async Task UpsertDailyAccessAsync(DailyDocumentAccess dailyAccess)
    {
        var existing = await GetByDocumentAndDateAsync(dailyAccess.DocumentId, dailyAccess.AccessDate);
        
        if (existing != null)
        {
            // Update existing record
            existing.ViewCount = dailyAccess.ViewCount;
            existing.DownloadCount = dailyAccess.DownloadCount;
            existing.SearchCount = dailyAccess.SearchCount;
            existing.TotalAccess = dailyAccess.TotalAccess;
            existing.UpdatedAt = DateTime.UtcNow;
            
            Update(existing);
        }
        else
        {
            // Add new record
            await AddAsync(dailyAccess);
        }
    }
}
