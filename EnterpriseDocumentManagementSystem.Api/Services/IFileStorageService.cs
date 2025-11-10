using EnterpriseDocumentManagementSystem.Api.Models.DTOs;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public interface IFileStorageService
{
    Task<FileUploadResult> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream FileStream, string ContentType)?> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
    long GetMaxFileSize();
    bool IsAllowedFileType(string contentType);
}
