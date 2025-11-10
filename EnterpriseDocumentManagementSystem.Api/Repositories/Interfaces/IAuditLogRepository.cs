using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetLogsByUserAsync(string userId, int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetLogsByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetLogsByActionTypeAsync(AuditActionType actionType, int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetFailedActionsAsync(int count, CancellationToken cancellationToken = default);
}
