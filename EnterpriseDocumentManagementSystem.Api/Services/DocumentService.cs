using AutoMapper;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Repositories.Interfaces;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DocumentService> _logger;
    private readonly IMapper _mapper;
    private readonly IAuthorizationService _authorizationService;

    public DocumentService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ILogger<DocumentService> logger,
        IMapper mapper,
        IAuthorizationService authorizationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    public async Task<DocumentResponse?> UploadDocumentAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        DocumentUploadRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Starting document upload. FileName: {FileName}, ContentType: {ContentType}, UserId: {UserId}", 
            fileName, contentType, userId);
        
        try
        {
            // Validate file type and size
            if (!_fileStorage.IsAllowedFileType(contentType))
            {
                _logger.LogWarning("Invalid file type attempted: {ContentType} by user {UserId}", contentType, userId);
                return null;
            }

            if (fileStream.Length > _fileStorage.GetMaxFileSize())
            {
                _logger.LogWarning("File size exceeds limit: {FileSize}", fileStream.Length);
                return null;
            }

            // Save file to storage
            var uploadResult = await _fileStorage.SaveFileAsync(fileStream, fileName, contentType, cancellationToken);
            if (!uploadResult.Success || uploadResult.FilePath == null)
            {
                _logger.LogError("File upload failed: {Error}", uploadResult.ErrorMessage);
                return null;
            }

            // Create document entity
            var document = new Document
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                FileName = fileName,
                FileSize = fileStream.Length,
                ContentType = contentType,
                FilePath = uploadResult.FilePath,
                AccessType = request.AccessType,
                UploadedBy = userId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _unitOfWork.Documents.AddAsync(document, cancellationToken);

            // Process tags
            if (request.Tags != null && request.Tags.Any())
            {
                await ProcessTagsAsync(document.Id, request.Tags, userId, cancellationToken);
            }

            // Log audit
            await LogAuditAsync(document.Id, userId, "Upload Document", AuditActionType.Create, 
                $"Uploaded document: {document.Title}", cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document uploaded successfully: {DocumentId}", document.Id);

            return await MapToDocumentResponse(document, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return null;
        }
    }

    public async Task<DocumentResponse?> GetDocumentByIdAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetDocumentWithDetailsAsync(documentId, cancellationToken);
        
        if (document == null)
        {
            return null;
        }

        // Check access
        if (!await HasAccessAsync(document, userId, cancellationToken))
        {
            await LogAuditAsync(documentId, userId, "Access Denied", AuditActionType.AccessDenied, 
                $"Attempted to access document: {document.Title}", cancellationToken, false);
            return null;
        }

        // Log read action
        await LogAuditAsync(documentId, userId, "View Document", AuditActionType.Read, 
            $"Viewed document: {document.Title}", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDocumentResponse(document, userId);
    }

    public async Task<PaginatedResponse<DocumentListResponse>> GetUserDocumentsAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        //TODO: this could be optimized and be via SP on the DB
        var allDocuments = await _unitOfWork.Documents.GetDocumentsByUserAsync(userId, cancellationToken);
        var totalCount = allDocuments.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var documents = allDocuments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDocumentListResponse(d))
            .ToList();

        return new PaginatedResponse<DocumentListResponse>
        {
            Items = documents,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<PaginatedResponse<DocumentListResponse>> GetSharedDocumentsAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var allDocuments = await _unitOfWork.Documents.GetSharedDocumentsAsync(userId, cancellationToken);
        var totalCount = allDocuments.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var documents = allDocuments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDocumentListResponse(d, true))
            .ToList();

        return new PaginatedResponse<DocumentListResponse>
        {
            Items = documents,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<PaginatedResponse<DocumentListResponse>> GetPublicDocumentsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var allDocuments = await _unitOfWork.Documents.GetPublicDocumentsAsync(cancellationToken);
        var totalCount = allDocuments.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var documents = allDocuments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => MapToDocumentListResponse(d))
            .ToList();

        return new PaginatedResponse<DocumentListResponse>
        {
            Items = documents,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<DocumentResponse?> UpdateDocumentAsync(
        Guid documentId,
        DocumentUpdateRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating document {DocumentId} by user {UserId}. Title: {Title}, Tags: {TagCount}", 
            documentId, userId, request.Title, request.Tags?.Count ?? 0);
        
        var document = await _unitOfWork.Documents.GetDocumentWithDetailsAsync(documentId, cancellationToken);
        
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found for update", documentId);
            return null;
        }

        // Check if user can edit
        if (!await CanEditAsync(document, userId, cancellationToken))
        {
            _logger.LogWarning("User {UserId} denied edit access to document {DocumentId}", userId, documentId);
            await LogAuditAsync(documentId, userId, "Edit Denied", AuditActionType.AccessDenied, 
                $"Attempted to edit document: {document.Title}", cancellationToken, false);
            return null;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            document.Title = request.Title;
        }

        if (request.Description != null)
        {
            document.Description = request.Description;
        }

        if (request.AccessType.HasValue)
        {
            document.AccessType = request.AccessType.Value;
        }

        document.LastModifiedDate = DateTime.UtcNow;

        await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

        // Update tags if provided
        if (request.Tags != null)
        {
            // Remove existing tags
            await _unitOfWork.DocumentTags.DeleteByDocumentIdAsync(document.Id, cancellationToken);

            // Add new tags
            await ProcessTagsAsync(document.Id, request.Tags, userId, cancellationToken);
        }

        // Log audit
        await LogAuditAsync(documentId, userId, "Update Document", AuditActionType.Update, 
            $"Updated document: {document.Title}", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document updated successfully: {DocumentId}", documentId);

        return await MapToDocumentResponse(document, userId);
    }

    public async Task<bool> DeleteDocumentAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting document {DocumentId} by user {UserId}", documentId, userId);
        
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null || document.IsDeleted)
        {
            _logger.LogWarning("Document {DocumentId} not found or already deleted", documentId);
            return false;
        }

        // Only owner can delete
        if (document.UploadedBy != userId)
        {
            _logger.LogWarning("User {UserId} denied delete access to document {DocumentId} (owner: {Owner})", 
                userId, documentId, document.UploadedBy);
            await LogAuditAsync(documentId, userId, "Delete Denied", AuditActionType.AccessDenied, 
                $"Attempted to delete document: {document.Title}", cancellationToken, false);
            return false;
        }

        // Soft delete
        document.IsDeleted = true;
        document.DeletedDate = DateTime.UtcNow;

        await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

        // Log audit
        await LogAuditAsync(documentId, userId, "Delete Document", AuditActionType.Delete, 
            $"Deleted document: {document.Title}", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Delete physical file (optional - you might want to keep it for recovery)
        if (!string.IsNullOrEmpty(document.FilePath))
        {
            await _fileStorage.DeleteFileAsync(document.FilePath, cancellationToken);
        }

        _logger.LogInformation("Document deleted successfully: {DocumentId}", documentId);

        return true;
    }

    public async Task<(Stream FileStream, string FileName, string ContentType)?> DownloadDocumentAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null || document.IsDeleted)
        {
            return null;
        }

        // Check access
        if (!await HasAccessAsync(document, userId, cancellationToken))
        {
            await LogAuditAsync(documentId, userId, "Download Denied", AuditActionType.AccessDenied, 
                $"Attempted to download document: {document.Title}", cancellationToken, false);
            return null;
        }

        if (string.IsNullOrEmpty(document.FilePath))
        {
            return null;
        }

        var fileResult = await _fileStorage.GetFileAsync(document.FilePath, cancellationToken);
        
        if (fileResult == null)
        {
            return null;
        }

        // Log download
        await LogAuditAsync(documentId, userId, "Download Document", AuditActionType.Download, 
            $"Downloaded document: {document.Title}", cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (fileResult.Value.FileStream, document.FileName, fileResult.Value.ContentType);
    }

    public async Task<DocumentSearchResponse> SearchDocumentsAsync(
        DocumentSearchRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var searchTerm = request.SearchTerm?.Trim().ToLowerInvariant();
        
        // Get all accessible documents
        var allDocuments = new List<Document>();
        
        // User's own documents
        var userDocs = await _unitOfWork.Documents.GetDocumentsByUserAsync(userId, cancellationToken);
        allDocuments.AddRange(userDocs);
        
        // Shared documents
        var sharedDocs = await _unitOfWork.Documents.GetSharedDocumentsAsync(userId, cancellationToken);
        allDocuments.AddRange(sharedDocs);
        
        // Public documents
        var publicDocs = await _unitOfWork.Documents.GetPublicDocumentsAsync(cancellationToken);
        allDocuments.AddRange(publicDocs);

        // Remove duplicates
        allDocuments = allDocuments.DistinctBy(d => d.Id).ToList();

        // Apply filters
        var filtered = allDocuments.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filtered = filtered.Where(d =>
                d.Title.ToLowerInvariant().Contains(searchTerm) ||
                (d.Description != null && d.Description.ToLowerInvariant().Contains(searchTerm)) ||
                d.FileName.ToLowerInvariant().Contains(searchTerm));
        }

        if (request.AccessType.HasValue)
        {
            filtered = filtered.Where(d => d.AccessType == request.AccessType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ContentType))
        {
            filtered = filtered.Where(d => d.ContentType.Contains(request.ContentType, StringComparison.OrdinalIgnoreCase));
        }

        if (request.Tags != null && request.Tags.Any())
        {
            filtered = filtered.Where(d => 
                d.DocumentTags.Any(dt => request.Tags.Contains(dt.Tag.Name, StringComparer.OrdinalIgnoreCase)));
        }

        var totalCount = filtered.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var pagedDocuments = filtered
            .OrderByDescending(d => d.CreatedDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => MapToDocumentListResponse(d))
            .ToList();

        return new DocumentSearchResponse
        {
            Documents = pagedDocuments,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<DocumentShareResponse?> ShareDocumentAsync(
        ShareDocumentRequest request,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.DocumentId, cancellationToken);
        
        if (document == null || document.IsDeleted)
        {
            return null;
        }

        // Only owner can share
        if (document.UploadedBy != userId)
        {
            await LogAuditAsync(request.DocumentId, userId, "Share Denied", AuditActionType.AccessDenied, 
                $"Attempted to share document: {document.Title}", cancellationToken, false);
            return null;
        }

        // Check if already shared
        var existingShare = await _unitOfWork.DocumentShares.GetActiveShareAsync(
            request.DocumentId, request.SharedWithUserId, cancellationToken);

        if (existingShare != null)
        {
            // Update existing share
            existingShare.PermissionLevel = request.PermissionLevel;
            existingShare.ExpirationDate = request.ExpirationDate;
            await _unitOfWork.DocumentShares.UpdateAsync(existingShare, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<DocumentShareResponse>(existingShare);
        }

        // Create new share
        var share = new DocumentShare
        {
            Id = Guid.NewGuid(),
            DocumentId = request.DocumentId,
            SharedWithUserId = request.SharedWithUserId,
            PermissionLevel = request.PermissionLevel,
            SharedBy = userId,
            SharedDate = DateTime.UtcNow,
            ExpirationDate = request.ExpirationDate,
            IsRevoked = false
        };

        await _unitOfWork.DocumentShares.AddAsync(share, cancellationToken);

        // Log audit
        await LogAuditAsync(request.DocumentId, userId, "Share Document", AuditActionType.Share, 
            $"Shared document with user: {request.SharedWithUserId}", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document shared successfully: {DocumentId} with {UserId}", 
            request.DocumentId, request.SharedWithUserId);

        return _mapper.Map<DocumentShareResponse>(share);
    }

    public async Task<bool> RevokeShareAsync(
        Guid shareId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var share = await _unitOfWork.DocumentShares.GetByIdAsync(shareId, cancellationToken);
        
        if (share == null || share.IsRevoked)
        {
            return false;
        }

        var document = await _unitOfWork.Documents.GetByIdAsync(share.DocumentId, cancellationToken);
        
        if (document == null || document.UploadedBy != userId)
        {
            return false;
        }

        await _unitOfWork.DocumentShares.RevokeShareAsync(shareId, userId, cancellationToken);

        // Log audit
        await LogAuditAsync(share.DocumentId, userId, "Revoke Share", AuditActionType.Update, 
            $"Revoked share for user: {share.SharedWithUserId}", cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Share revoked successfully: {ShareId}", shareId);

        return true;
    }

    public async Task<List<DocumentShareResponse>> GetDocumentSharesAsync(
        Guid documentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        
        if (document == null || document.UploadedBy != userId)
        {
            return new List<DocumentShareResponse>();
        }

        var shares = await _unitOfWork.DocumentShares.GetSharesByDocumentAsync(documentId, cancellationToken);
        return _mapper.Map<List<DocumentShareResponse>>(shares);
    }

    // Helper methods
    private async Task<bool> HasAccessAsync(Document document, string userId, CancellationToken cancellationToken)
    {
        // Owner always has access
        if (document.UploadedBy == userId)
        {
            return true;
        }

        // Public: All authenticated users can access
        if (document.AccessType == AccessType.Public)
        {
            return true;
        }

        // Private: Only the document owner can access
        if (document.AccessType == AccessType.Private)
        {
            return false; // Already checked owner above
        }

        // Restricted: Only specific users with active shares can access
        if (document.AccessType == AccessType.Restricted)
        {
            return await _unitOfWork.DocumentShares.HasActiveShareAsync(document.Id, userId, cancellationToken);
        }

        return false;
    }

    private async Task<bool> CanEditAsync(Document document, string userId, CancellationToken cancellationToken)
    {
        if (document.UploadedBy == userId)
        {
            return true;
        }

        var share = await _unitOfWork.DocumentShares.GetActiveShareAsync(document.Id, userId, cancellationToken);
        return share != null && (share.PermissionLevel == PermissionLevel.Edit || share.PermissionLevel == PermissionLevel.FullControl);
    }

    private async Task ProcessTagsAsync(Guid documentId, List<string> tagNames, string userId, CancellationToken cancellationToken)
    {
        foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var tag = await _unitOfWork.Tags.GetByNameAsync(tagName.Trim(), cancellationToken);
            
            if (tag == null)
            {
                tag = new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = tagName.Trim(),
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            var documentTag = new DocumentTag
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                TagId = tag.Id,
                AssignedBy = userId,
                AssignedDate = DateTime.UtcNow
            };

            await _unitOfWork.DocumentTags.AddAsync(documentTag, cancellationToken);
        }
    }

    private async Task LogAuditAsync(
        Guid? documentId,
        string userId,
        string action,
        AuditActionType actionType,
        string? details,
        CancellationToken cancellationToken,
        bool isSuccessful = true)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            UserId = userId,
            Action = action,
            ActionType = actionType,
            Details = details,
            Timestamp = DateTime.UtcNow,
            IsSuccessful = isSuccessful
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    private async Task<DocumentResponse> MapToDocumentResponse(Document document, string userId)
    {
        var canEdit = await CanEditAsync(document, userId, default);
        var isOwner = document.UploadedBy == userId;

        var response = _mapper.Map<DocumentResponse>(document);
        response.CanEdit = canEdit;
        response.CanDelete = isOwner;
        response.CanShare = isOwner;

        return response;
    }

    private DocumentListResponse MapToDocumentListResponse(Document document, bool isShared = false)
    {
        var response = _mapper.Map<DocumentListResponse>(document);
        response.IsShared = isShared;
        return response;
    }
}
