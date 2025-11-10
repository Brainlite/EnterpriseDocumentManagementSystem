using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetDocumentsByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetPublicDocumentsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetSharedDocumentsAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> GetDocumentsByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm, string? userId = null, CancellationToken cancellationToken = default);
    Task<Document?> GetDocumentWithDetailsAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessToDocumentAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
}
