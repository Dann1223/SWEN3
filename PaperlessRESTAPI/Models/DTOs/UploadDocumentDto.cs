using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// DTO for document upload requests
/// </summary>
public class UploadDocumentDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;

    public List<int> TagIds { get; set; } = new();
}
