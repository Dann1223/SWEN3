using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Models.DTOs.Comments;

/// <summary>
/// DTO for creating a new comment
/// </summary>
public class CreateCommentDto
{
    /// <summary>
    /// Document ID to comment on
    /// </summary>
    [Required]
    public int DocumentId { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Comment content
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Parent comment ID for replies (optional)
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// Position in document for annotations (optional)
    /// </summary>
    [MaxLength(100)]
    public string? Position { get; set; }
}

/// <summary>
/// DTO for updating a comment
/// </summary>
public class UpdateCommentDto
{
    /// <summary>
    /// Updated comment content
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// DTO for comment response
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Comment ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Document ID
    /// </summary>
    public int DocumentId { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    /// Comment content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Parent comment ID
    /// </summary>
    public int? ParentCommentId { get; set; }

    /// <summary>
    /// Position in document
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Whether comment is edited
    /// </summary>
    public bool IsEdited { get; set; }

    /// <summary>
    /// Number of replies
    /// </summary>
    public int ReplyCount { get; set; }

    /// <summary>
    /// Nested replies (for hierarchical display)
    /// </summary>
    public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
}
