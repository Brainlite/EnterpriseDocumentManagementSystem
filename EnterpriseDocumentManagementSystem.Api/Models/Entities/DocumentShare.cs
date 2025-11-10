using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseDocumentManagementSystem.Api.Models.Entities;

public class DocumentShare
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SharedWithUserId { get; set; } = string.Empty;

    [Required]
    public PermissionLevel PermissionLevel { get; set; }

    [Required]
    [MaxLength(100)]
    public string SharedBy { get; set; } = string.Empty;

    [Required]
    public DateTime SharedDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedDate { get; set; }

    [MaxLength(100)]
    public string? RevokedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;
}

public enum PermissionLevel
{
    View = 0,
    Edit = 1,
    FullControl = 2
}
