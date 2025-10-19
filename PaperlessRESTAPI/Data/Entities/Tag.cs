using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Tag entity for categorizing documents
/// </summary>
public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(7)] // For hex color codes like #FF5733
    public string Color { get; set; } = "#007bff";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
