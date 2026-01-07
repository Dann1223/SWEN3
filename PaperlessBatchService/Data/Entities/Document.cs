using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessBatchService.Data.Entities;

/// <summary>
/// Document entity representing a file in the document management system
/// </summary>
public class Document
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastModified { get; set; }

    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [Column(TypeName = "text")]
    public string? Content { get; set; }

    [Column(TypeName = "text")]
    public string? Summary { get; set; }

    public bool IsProcessed { get; set; } = false;

    public bool IsIndexed { get; set; } = false;

    // AI Processing fields
    public bool IsAIProcessed { get; set; } = false;
    
    public DateTime? AIProcessedAt { get; set; }
    
    [MaxLength(500)]
    public string? AIErrorMessage { get; set; }

    public float? Confidence { get; set; }

    // Navigation properties
    public virtual ICollection<DailyDocumentAccess> DailyAccess { get; set; } = new List<DailyDocumentAccess>();
}
