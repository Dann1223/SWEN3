using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Repository interface for batch processing history
/// </summary>
public interface IBatchProcessingHistoryRepository : IRepository<BatchProcessingHistory>
{
    Task<BatchProcessingHistory?> GetByFileNameAsync(string fileName);
    Task<IEnumerable<BatchProcessingHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsFileProcessedAsync(string fileName, string? checksum = null);
}
