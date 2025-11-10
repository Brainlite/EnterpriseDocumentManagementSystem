using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseDocumentManagementSystem.Api.Models.Entities;

public class Document
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FilePath { get; set; }

    [Required]
    public AccessType AccessType { get; set; }

    [Required]
    [MaxLength(100)]
    public string UploadedBy { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public DateTime LastModifiedDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    // Navigation properties
    public virtual ICollection<DocumentShare> DocumentShares { get; set; } = new List<DocumentShare>();
    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}

public enum AccessType
{
    Public = 0,
    Private = 1,
    Restricted = 2
}
