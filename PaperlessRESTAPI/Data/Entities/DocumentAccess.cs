using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Document access log entity for tracking document access statistics
/// </summary>
public class DocumentAccess
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public DateTime AccessDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty; // View, Download, Search, etc.

    // Navigation properties
    public virtual Document Document { get; set; } = null!;
}
