using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class DocumentTagRepository : Repository<DocumentTag>, IDocumentTagRepository
{
    public DocumentTagRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DocumentTag>> GetByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(dt => dt.DocumentId == documentId)
            .Include(dt => dt.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var documentTags = await _dbSet
            .Where(dt => dt.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(documentTags);
    }
}
