using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByUserAsync(
        string userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByActionTypeAsync(
        AuditActionType actionType,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ActionType == actionType)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetFailedActionsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => !a.IsSuccessful)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
