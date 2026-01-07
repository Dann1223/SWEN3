using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Repository implementation for batch processing history
/// </summary>
public class BatchProcessingHistoryRepository : Repository<BatchProcessingHistory>, IBatchProcessingHistoryRepository
{
    public BatchProcessingHistoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<BatchProcessingHistory?> GetByFileNameAsync(string fileName)
    {
        return await _context.BatchProcessingHistories
            .Where(b => b.FileName == fileName)
            .OrderByDescending(b => b.ProcessedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<BatchProcessingHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.BatchProcessingHistories
            .Where(b => b.ProcessedAt >= startDate && b.ProcessedAt <= endDate)
            .OrderByDescending(b => b.ProcessedAt)
            .ToListAsync();
    }

    public async Task<bool> IsFileProcessedAsync(string fileName, string? checksum = null)
    {
        var query = _context.BatchProcessingHistories
            .Where(b => b.FileName == fileName && b.IsSuccessful);

        if (!string.IsNullOrEmpty(checksum))
        {
            query = query.Where(b => b.FileChecksum == checksum);
        }

        return await query.AnyAsync();
    }
}
