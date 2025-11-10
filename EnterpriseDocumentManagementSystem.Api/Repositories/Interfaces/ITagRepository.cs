using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetTagsByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default);
}
