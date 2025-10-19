using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessRESTAPI.Data.Entities;

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
    public string? OcrText { get; set; }

    [Column(TypeName = "text")]
    public string? Summary { get; set; }

    public bool IsProcessed { get; set; } = false;

    public bool IsIndexed { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public virtual ICollection<DocumentAccess> AccessLogs { get; set; } = new List<DocumentAccess>();
}
