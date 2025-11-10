using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnterpriseDocumentManagementSystem.Api.Models.Entities;

public class DocumentTag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid DocumentId { get; set; }

    [Required]
    public Guid TagId { get; set; }

    [Required]
    public DateTime AssignedDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string AssignedBy { get; set; } = string.Empty;

    // Navigation properties
    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(TagId))]
    public virtual Tag Tag { get; set; } = null!;
}
