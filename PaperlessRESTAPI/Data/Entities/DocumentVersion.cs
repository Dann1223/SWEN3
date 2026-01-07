using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Document version entity for version control system
/// </summary>
[Table("DocumentVersions")]
public class DocumentVersion
{
    /// <summary>
    /// Unique identifier for the version
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Document ID
    /// </summary>
    [Required]
    public int DocumentId { get; set; }

    /// <summary>
    /// Version number (1, 2, 3, etc.)
    /// </summary>
    [Required]
    public int VersionNumber { get; set; }

    /// <summary>
    /// File path in storage
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Original file name
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// File type/extension
    /// </summary>
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// User who created this version
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }

    /// <summary>
    /// When the version was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Change description/notes
    /// </summary>
    [MaxLength(1000)]
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Whether this is the current active version
    /// </summary>
    public bool IsCurrentVersion { get; set; } = false;

    /// <summary>
    /// OCR text for this version
    /// </summary>
    public string? OcrText { get; set; }

    /// <summary>
    /// Whether OCR processing is completed
    /// </summary>
    public bool IsProcessed { get; set; } = false;

    /// <summary>
    /// Processing confidence score
    /// </summary>
    public float? Confidence { get; set; }

    // Navigation properties
    /// <summary>
    /// Reference to the document
    /// </summary>
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Reference to the user who created this version
    /// </summary>
    [ForeignKey("CreatedBy")]
    public virtual User CreatedByUser { get; set; } = null!;
}
