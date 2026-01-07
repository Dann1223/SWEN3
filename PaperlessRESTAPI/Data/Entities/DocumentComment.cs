using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Document comment entity for collaboration
/// </summary>
[Table("DocumentComments")]
public class DocumentComment
{
    /// <summary>
    /// Unique identifier for the comment
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Document ID
    /// </summary>
    [Required]
    public int DocumentId { get; set; }

    /// <summary>
    /// Author name (simple string instead of user system)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = "Anonymous";

    /// <summary>
    /// Comment content
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Parent comment ID for replies
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// When the comment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the comment was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the comment has been edited
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Whether the comment is deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Page number or section reference (optional)
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// Position on page for annotations (JSON format)
    /// </summary>
    [MaxLength(500)]
    public string? Position { get; set; }

        // Navigation properties
    /// <summary>
    /// Reference to the document
    /// </summary>
    [ForeignKey("DocumentId")]
    public virtual Document Document { get; set; } = null!;

    /// <summary>
    /// Reference to parent comment for threading
    /// </summary>
    [ForeignKey("ParentCommentId")]
    public virtual DocumentComment? ParentComment { get; set; }

    /// <summary>
    /// Child replies to this comment
    /// </summary>
    public virtual ICollection<DocumentComment> Replies { get; set; } = new List<DocumentComment>();
}
