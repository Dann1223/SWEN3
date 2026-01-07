namespace PaperlessRESTAPI.Models.DTOs;

/// <summary>
/// Data Transfer Object for creating a new tag
/// </summary>
public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#007bff";
}
