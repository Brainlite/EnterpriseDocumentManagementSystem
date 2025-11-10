using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Models.DTOs;

public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccessType AccessType { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class DocumentUpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public AccessType? AccessType { get; set; }
    public List<string>? Tags { get; set; }
}

public class DocumentResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public AccessType AccessType { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<TagResponse> Tags { get; set; } = new();
    public List<DocumentShareResponse> Shares { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanShare { get; set; }
}

public class DocumentListResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public AccessType AccessType { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public List<string> TagNames { get; set; } = new();
    public bool IsShared { get; set; }
}

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DocumentSearchRequest
{
    public string? SearchTerm { get; set; }
    public List<string>? Tags { get; set; }
    public AccessType? AccessType { get; set; }
    public string? ContentType { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

public class DocumentSearchResponse
{
    public List<DocumentListResponse> Documents { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ShareDocumentRequest
{
    public Guid DocumentId { get; set; }
    public string SharedWithUserId { get; set; } = string.Empty;
    public PermissionLevel PermissionLevel { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class DocumentShareResponse
{
    public Guid Id { get; set; }
    public string SharedWithUserId { get; set; } = string.Empty;
    public PermissionLevel PermissionLevel { get; set; }
    public string SharedBy { get; set; } = string.Empty;
    public DateTime SharedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsRevoked { get; set; }
}

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class FileUploadResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
}
