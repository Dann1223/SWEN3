using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// User entity for document collaboration system
/// </summary>
[Table("Users")]
public class User
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique username for authentication
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's password hash
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User creation date
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User's last login date
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// User's role (Admin, Editor, Viewer)
    /// </summary>
    [MaxLength(20)]
    public string Role { get; set; } = "Viewer";

    // Navigation properties
    /// <summary>
    /// Document permissions for this user
    /// </summary>
    public virtual ICollection<DocumentPermission> DocumentPermissions { get; set; } = new List<DocumentPermission>();

    /// <summary>
    /// Comments made by this user
    /// </summary>
    public virtual ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();

    /// <summary>
    /// Document versions created by this user
    /// </summary>
    public virtual ICollection<DocumentVersion> CreatedVersions { get; set; } = new List<DocumentVersion>();
}
