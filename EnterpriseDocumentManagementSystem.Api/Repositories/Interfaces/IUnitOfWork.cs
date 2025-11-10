namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IDocumentRepository Documents { get; }
    IDocumentShareRepository DocumentShares { get; }
    ITagRepository Tags { get; }
    IDocumentTagRepository DocumentTags { get; }
    IAuditLogRepository AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
