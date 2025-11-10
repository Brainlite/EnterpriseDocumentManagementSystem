using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetDocumentsByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Where(d => d.UploadedBy == userId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetPublicDocumentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Where(d => d.AccessType == AccessType.Public && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetSharedDocumentsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.DocumentShares.Where(s => s.SharedWithUserId == userId && !s.IsRevoked))
            .Where(d => !d.IsDeleted &&
                        d.DocumentShares.Any(s => s.SharedWithUserId == userId && !s.IsRevoked))
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> GetDocumentsByTagAsync(
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Where(d => !d.IsDeleted &&
                        d.DocumentTags.Any(dt => dt.TagId == tagId))
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Document>> SearchDocumentsAsync(
        string searchTerm,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(d =>
                d.Title.Contains(searchTerm) ||
                (d.Description != null && d.Description.Contains(searchTerm)) ||
                d.FileName.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(d =>
                d.UploadedBy == userId ||
                d.AccessType == AccessType.Public ||
                d.DocumentShares.Any(s => s.SharedWithUserId == userId && !s.IsRevoked));
        }

        return await query
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Document?> GetDocumentWithDetailsAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.DocumentShares.Where(s => !s.IsRevoked))
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasAccessToDocumentAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(d =>
            d.Id == documentId &&
            !d.IsDeleted &&
            (d.UploadedBy == userId ||
             d.AccessType == AccessType.Public ||
             d.DocumentShares.Any(s => s.SharedWithUserId == userId && !s.IsRevoked)),
            cancellationToken);
    }
}
