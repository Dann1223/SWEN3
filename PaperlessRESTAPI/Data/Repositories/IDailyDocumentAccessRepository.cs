using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Repository interface for daily document access statistics
/// </summary>
public interface IDailyDocumentAccessRepository : IRepository<DailyDocumentAccess>
{
    Task<DailyDocumentAccess?> GetByDocumentAndDateAsync(int documentId, DateOnly date);
    Task<IEnumerable<DailyDocumentAccess>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<IEnumerable<DailyDocumentAccess>> GetByDocumentIdAsync(int documentId);
    Task UpsertDailyAccessAsync(DailyDocumentAccess dailyAccess);
}
