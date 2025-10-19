namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Unit of Work interface for managing repositories and transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDocumentRepository Documents { get; }
    IRepository<Entities.Tag> Tags { get; }
    IRepository<Entities.DocumentAccess> DocumentAccesses { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
