using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Models.DTOs;

public class CreateDocumentDto
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    
    public List<int> TagIds { get; set; } = new();
}

public class UpdateDocumentDto
{
    [StringLength(255)]
    public string? Title { get; set; }

    public string? Description { get; set; }
    
    public string? Summary { get; set; }
    
    public List<int> TagIds { get; set; } = new();
}
