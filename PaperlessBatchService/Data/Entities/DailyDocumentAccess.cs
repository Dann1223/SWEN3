using System.ComponentModel.DataAnnotations;

namespace PaperlessBatchService.Data.Entities;

/// <summary>
/// Daily document access statistics entity for batch processing
/// </summary>
public class DailyDocumentAccess
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public DateOnly AccessDate { get; set; }

    [Required]
    public int ViewCount { get; set; } = 0;

    [Required]
    public int DownloadCount { get; set; } = 0;

    [Required]
    public int SearchCount { get; set; } = 0;

    [Required]
    public int TotalAccess { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Document Document { get; set; } = null!;
}
