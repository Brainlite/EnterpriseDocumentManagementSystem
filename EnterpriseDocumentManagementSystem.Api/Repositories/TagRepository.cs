using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Data;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Repositories;

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetTagsByDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.DocumentTags.Any(dt => dt.DocumentId == documentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(t => t.DocumentTags.Count)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
