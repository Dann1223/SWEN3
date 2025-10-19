namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Tag Data Transfer Object for API responses
/// </summary>
public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#007bff";
    public DateTime CreatedDate { get; set; }
}
