using Microsoft.AspNetCore.Mvc;

namespace EnterpriseDocumentManagementSystem.Api.Models.DTOs;

public class DocumentUploadFormRequest
{
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = null!;

    [FromForm(Name = "title")]
    public string Title { get; set; } = string.Empty;

    [FromForm(Name = "description")]
    public string? Description { get; set; }

    [FromForm(Name = "accessType")]
    public string AccessType { get; set; } = string.Empty;

    [FromForm(Name = "tags")]
    public string? Tags { get; set; }
}
