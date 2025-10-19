using Microsoft.EntityFrameworkCore.Storage;

namespace PaperlessRESTAPI.Data.Repositories;

/// <summary>
/// Unit of Work implementation for managing repositories and transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IDocumentRepository? _documents;
    private IRepository<Entities.Tag>? _tags;
    private IRepository<Entities.DocumentAccess>? _documentAccesses;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IDocumentRepository Documents =>
        _documents ??= new DocumentRepository(_context);

    public IRepository<Entities.Tag> Tags =>
        _tags ??= new Repository<Entities.Tag>(_context);

    public IRepository<Entities.DocumentAccess> DocumentAccesses =>
        _documentAccesses ??= new Repository<Entities.DocumentAccess>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
