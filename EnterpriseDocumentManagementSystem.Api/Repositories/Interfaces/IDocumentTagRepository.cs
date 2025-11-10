using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface IDocumentTagRepository : IRepository<DocumentTag>
{
    Task<IEnumerable<DocumentTag>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task DeleteByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
}
