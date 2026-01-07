using System.ComponentModel.DataAnnotations;

namespace PaperlessRESTAPI.Data.Entities;

/// <summary>
/// Batch processing history entity for tracking XML file processing
/// </summary>
public class BatchProcessingHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsSuccessful { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    [Required]
    public int RecordsProcessed { get; set; } = 0;

    [Required]
    public long FileSizeBytes { get; set; }

    [MaxLength(64)]
    public string? FileChecksum { get; set; }
}
