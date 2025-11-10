using Microsoft.EntityFrameworkCore.Storage;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        ApplicationDbContext context,
        IDocumentRepository documents,
        IDocumentShareRepository documentShares,
        ITagRepository tags,
        IDocumentTagRepository documentTags,
        IAuditLogRepository auditLogs)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Documents = documents ?? throw new ArgumentNullException(nameof(documents));
        DocumentShares = documentShares ?? throw new ArgumentNullException(nameof(documentShares));
        Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        DocumentTags = documentTags ?? throw new ArgumentNullException(nameof(documentTags));
        AuditLogs = auditLogs ?? throw new ArgumentNullException(nameof(auditLogs));
    }

    public IDocumentRepository Documents { get; }
    public IDocumentShareRepository DocumentShares { get; }
    public ITagRepository Tags { get; }
    public IDocumentTagRepository DocumentTags { get; }
    public IAuditLogRepository AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
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
