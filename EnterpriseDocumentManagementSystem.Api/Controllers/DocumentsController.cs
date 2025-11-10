using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Services;
using EnterpriseDocumentManagementSystem.Api.Filters;
using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Extensions;

namespace EnterpriseDocumentManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(CurrentUserFilter))]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Upload a new document (Contributor role or higher required)
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = Roles.ContributorOrHigher)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadFormRequest formRequest)
    {
        _logger.LogDebug("Upload request received. FileName: {FileName}, Size: {Size}", 
            formRequest.File?.FileName, formRequest.File?.Length);
        
        if (formRequest.File == null || formRequest.File.Length == 0)
        {
            _logger.LogWarning("Upload rejected: No file provided");
            return BadRequest(new { error = "No file provided" });
        }

        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Upload rejected: User not authenticated");
            return Unauthorized(new { error = "User not authenticated" });
        }
        
        _logger.LogDebug("Processing upload for user {UserId}", userId);

        // Parse access type
        if (!Enum.TryParse<Models.Entities.AccessType>(formRequest.AccessType, true, out var parsedAccessType))
        {
            return BadRequest(new { error = "Invalid access type" });
        }

        // Parse tags
        var tagList = string.IsNullOrWhiteSpace(formRequest.Tags)
            ? new List<string>()
            : formRequest.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var request = new DocumentUploadRequest
        {
            Title = formRequest.Title,
            Description = formRequest.Description,
            AccessType = parsedAccessType,
            Tags = tagList
        };

        using var stream = formRequest.File.OpenReadStream();
        var result = await _documentService.UploadDocumentAsync(
            stream,
            formRequest.File.FileName,
            formRequest.File.ContentType,
            request,
            userId);

        if (result == null)
        {
            return BadRequest(new { error = "Failed to upload document. Please check file type and size." });
        }

        return CreatedAtAction(nameof(GetDocument), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var document = await _documentService.GetDocumentByIdAsync(id, userId);

        if (document == null)
        {
            return NotFound(new { error = "Document not found or access denied" });
        }

        return Ok(document);
    }

    /// <summary>
    /// Get all documents owned by the current user
    /// </summary>
    [HttpGet("my-documents")]
    [ProducesResponseType(typeof(PaginatedResponse<DocumentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyDocuments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var documents = await _documentService.GetUserDocumentsAsync(userId, pageNumber, pageSize);
        return Ok(documents);
    }

    /// <summary>
    /// Get documents shared with the current user
    /// </summary>
    [HttpGet("shared-with-me")]
    [ProducesResponseType(typeof(PaginatedResponse<DocumentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSharedDocuments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var documents = await _documentService.GetSharedDocumentsAsync(userId, pageNumber, pageSize);
        return Ok(documents);
    }

    /// <summary>
    /// Get all public documents
    /// </summary>
    [HttpGet("public")]
    [ProducesResponseType(typeof(PaginatedResponse<DocumentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicDocuments([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var documents = await _documentService.GetPublicDocumentsAsync(pageNumber, pageSize);
        return Ok(documents);
    }

    /// <summary>
    /// Search documents
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(DocumentSearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchDocuments([FromBody] DocumentSearchRequest request)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var results = await _documentService.SearchDocumentsAsync(request, userId);
        return Ok(results);
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var success = await _documentService.DeleteDocumentAsync(id, userId);

        if (!success)
        {
            return NotFound(new { error = "Document not found or you don't have permission to delete" });
        }

        return NoContent();
    }

    /// <summary>
    /// Update document metadata (title, description, tags, access type)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] DocumentUpdateRequest request)
    {
        _logger.LogDebug("Update request for document {DocumentId}", id);
        
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Update rejected: User not authenticated");
            return Unauthorized();
        }

        var result = await _documentService.UpdateDocumentAsync(id, request, userId);

        if (result == null)
        {
            _logger.LogWarning("Update failed for document {DocumentId} by user {UserId}", id, userId);
            return NotFound(new { error = "Document not found or you don't have permission to edit it" });
        }

        _logger.LogInformation("Document {DocumentId} updated successfully by user {UserId}", id, userId);
        return Ok(result);
    }

    /// <summary>
    /// Download a document
    /// </summary>
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _documentService.DownloadDocumentAsync(id, userId);

        if (result == null)
        {
            return NotFound(new { error = "Document not found or access denied" });
        }

        return File(result.Value.FileStream, result.Value.ContentType, result.Value.FileName);
    }

    /// <summary>
    /// Share a document with another user
    /// </summary>
    [HttpPost("share")]
    [ProducesResponseType(typeof(DocumentShareResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ShareDocument([FromBody] ShareDocumentRequest request)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var share = await _documentService.ShareDocumentAsync(request, userId);

        if (share == null)
        {
            return BadRequest(new { error = "Failed to share document. You may not have permission or the document doesn't exist." });
        }

        return CreatedAtAction(nameof(GetDocumentShares), new { documentId = request.DocumentId }, share);
    }

    /// <summary>
    /// Get all shares for a document
    /// </summary>
    [HttpGet("{documentId}/shares")]
    [ProducesResponseType(typeof(List<DocumentShareResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDocumentShares(Guid documentId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var shares = await _documentService.GetDocumentSharesAsync(documentId, userId);
        return Ok(shares);
    }

    /// <summary>
    /// Revoke a document share
    /// </summary>
    [HttpDelete("shares/{shareId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokeShare(Guid shareId)
    {
        var userId = HttpContext.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var success = await _documentService.RevokeShareAsync(shareId, userId);

        if (!success)
        {
            return NotFound(new { error = "Share not found or you don't have permission to revoke" });
        }

        return NoContent();
    }
}
