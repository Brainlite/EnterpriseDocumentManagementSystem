using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface IDocumentShareRepository : IRepository<DocumentShare>
{
    Task<IEnumerable<DocumentShare>> GetSharesByDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DocumentShare>> GetSharesByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<DocumentShare?> GetActiveShareAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
    Task RevokeShareAsync(Guid shareId, string revokedBy, CancellationToken cancellationToken = default);
    Task<bool> HasActiveShareAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
}
