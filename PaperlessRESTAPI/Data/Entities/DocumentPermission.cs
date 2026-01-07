using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Document permission entity for collaboration system
/// </summary>
[Table("DocumentPermissions")]
public class DocumentPermission
{
    /// <summary>
    /// Unique identifier for the permission
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Document ID
    /// </summary>
    [Required]
    public int DocumentId { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Permission level (Read, Write, Admin)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string PermissionLevel { get; set; } = "Read";

    /// <summary>
    /// When the permission was granted
    /// </summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who granted this permission
    /// </summary>
    public int? GrantedBy { get; set; }

    /// <summary>
    /// Whether the permission is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Reference to the document
    /// </summary>
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Reference to the user
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Reference to the user who granted the permission
    /// </summary>
    [ForeignKey("GrantedBy")]
    public virtual User? GrantedByUser { get; set; }
}
