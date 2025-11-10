using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class DocumentShareRepository : Repository<DocumentShare>, IDocumentShareRepository
{
    public DocumentShareRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DocumentShare>> GetSharesByDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DocumentId == documentId && !s.IsRevoked)
            .Include(s => s.Document)
            .OrderByDescending(s => s.SharedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DocumentShare>> GetSharesByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.SharedWithUserId == userId && !s.IsRevoked)
            .Include(s => s.Document)
            .OrderByDescending(s => s.SharedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<DocumentShare?> GetActiveShareAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s =>
                s.DocumentId == documentId &&
                s.SharedWithUserId == userId &&
                !s.IsRevoked &&
                (s.ExpirationDate == null || s.ExpirationDate > DateTime.UtcNow),
                cancellationToken);
    }

    public async Task RevokeShareAsync(
        Guid shareId,
        string revokedBy,
        CancellationToken cancellationToken = default)
    {
        var share = await GetByIdAsync(shareId, cancellationToken);
        if (share != null && !share.IsRevoked)
        {
            share.IsRevoked = true;
            share.RevokedDate = DateTime.UtcNow;
            share.RevokedBy = revokedBy;
            await UpdateAsync(share, cancellationToken);
        }
    }

    public async Task<bool> HasActiveShareAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(s =>
            s.DocumentId == documentId &&
            s.SharedWithUserId == userId &&
            !s.IsRevoked &&
            (s.ExpirationDate == null || s.ExpirationDate > DateTime.UtcNow),
            cancellationToken);
    }
}
