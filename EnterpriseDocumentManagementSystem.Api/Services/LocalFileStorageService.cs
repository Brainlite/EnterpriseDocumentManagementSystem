using EnterpriseDocumentManagementSystem.Api.Models.DTOs;

namespace EnterpriseDocumentManagementSystem.Api.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly long _maxFileSize;
    private readonly HashSet<string> _allowedContentTypes;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get storage path from configuration or use default
        _storagePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "DocumentStorage");
        
        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created document storage directory at: {StoragePath}", _storagePath);
        }

        // Max file size: 10MB
        _maxFileSize = configuration.GetValue<long>("FileStorage:MaxFileSizeBytes", 10 * 1024 * 1024);

        // Allowed file types
        _allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
            "application/msword", // .doc
            "text/plain"
        };
    }

    public long GetMaxFileSize() => _maxFileSize;

    public bool IsAllowedFileType(string contentType)
    {
        return _allowedContentTypes.Contains(contentType);
    }

    public async Task<FileUploadResult> SaveFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file type
            if (!IsAllowedFileType(contentType))
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File type '{contentType}' is not allowed. Allowed types: PDF, DOCX, TXT"
                };
            }

            // Validate file size
            if (fileStream.Length > _maxFileSize)
            {
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = $"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB"
                };
            }

            // Generate unique file name to avoid collisions
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Create subdirectory based on current date for better organization
            var dateFolder = DateTime.UtcNow.ToString("yyyy-MM");
            var fullDirectory = Path.Combine(_storagePath, dateFolder);
            
            if (!Directory.Exists(fullDirectory))
            {
                Directory.CreateDirectory(fullDirectory);
            }

            var fullPath = Path.Combine(fullDirectory, uniqueFileName);
            var relativePath = Path.Combine(dateFolder, uniqueFileName);

            // Save file
            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            _logger.LogInformation("File saved successfully: {FilePath}", relativePath);

            return new FileUploadResult
            {
                Success = true,
                FilePath = relativePath
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", fileName);
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = "An error occurred while saving the file"
            };
        }
    }

    public async Task<(Stream FileStream, string ContentType)?> GetFileAsync(
        string filePath, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                return null;
            }

            // Determine content type from file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (fileStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
            return null;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }
}
