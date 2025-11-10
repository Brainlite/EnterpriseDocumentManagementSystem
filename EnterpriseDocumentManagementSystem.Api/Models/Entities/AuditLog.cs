using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseDocumentManagementSystem.Api.Models.Entities;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    public Guid? DocumentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    public AuditActionType ActionType { get; set; }

    [MaxLength(500)]
    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    [MaxLength(2000)]
    public string? Details { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Required]
    public DateTime Timestamp { get; set; }

    public bool IsSuccessful { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    // Navigation properties
    [ForeignKey(nameof(DocumentId))]
    public virtual Document? Document { get; set; }
}

public enum AuditActionType
{
    Create = 0,
    Read = 1,
    Update = 2,
    Delete = 3,
    Share = 4,
    Download = 5,
    Login = 6,
    Logout = 7,
    AccessDenied = 8
}
