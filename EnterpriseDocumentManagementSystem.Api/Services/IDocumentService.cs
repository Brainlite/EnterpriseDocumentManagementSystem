using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public interface IDocumentService
{
    Task<DocumentResponse?> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, DocumentUploadRequest request, string userId, CancellationToken cancellationToken = default);
    Task<DocumentResponse?> GetDocumentByIdAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<DocumentListResponse>> GetUserDocumentsAsync(string userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<DocumentListResponse>> GetSharedDocumentsAsync(string userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<DocumentListResponse>> GetPublicDocumentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<DocumentResponse?> UpdateDocumentAsync(Guid documentId, DocumentUpdateRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
    Task<(Stream FileStream, string FileName, string ContentType)?> DownloadDocumentAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
    Task<DocumentSearchResponse> SearchDocumentsAsync(DocumentSearchRequest request, string userId, CancellationToken cancellationToken = default);
    Task<DocumentShareResponse?> ShareDocumentAsync(ShareDocumentRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeShareAsync(Guid shareId, string userId, CancellationToken cancellationToken = default);
    Task<List<DocumentShareResponse>> GetDocumentSharesAsync(Guid documentId, string userId, CancellationToken cancellationToken = default);
}
