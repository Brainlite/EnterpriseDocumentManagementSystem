using System.ComponentModel.DataAnnotations;

namespace EnterpriseDocumentManagementSystem.Api.Models.Entities;

public class Tag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Color { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public virtual ICollection<DocumentTag> DocumentTags { get; set; } = new List<DocumentTag>();
}
