namespace PaperlessBatchService.Configuration;

/// <summary>
/// Configuration options for batch processing service
/// </summary>
public class BatchProcessingOptions
{
    public const string SectionName = "BatchProcessing";

    /// <summary>
    /// Input folder path where XML files are placed for processing
    /// </summary>
    public string InputFolder { get; set; } = "/app/batch/input";

    /// <summary>
    /// Archive folder path for successfully processed files
    /// </summary>
    public string ArchiveFolder { get; set; } = "/app/batch/archive";

    /// <summary>
    /// Error folder path for files that failed processing
    /// </summary>
    public string ErrorFolder { get; set; } = "/app/batch/error";

    /// <summary>
    /// File name pattern for XML files to process (supports wildcards)
    /// </summary>
    public string FilePattern { get; set; } = "access_log_*.xml";

    /// <summary>
    /// Cron expression for scheduling the batch job (default: daily at 1:00 AM)
    /// </summary>
    public string CronSchedule { get; set; } = "0 1 * * *";

    /// <summary>
    /// Whether the batch processing service is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Maximum number of files to process in a single batch run
    /// </summary>
    public int MaxFilesPerBatch { get; set; } = 100;

    /// <summary>
    /// Maximum file size in bytes (default: 50MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
}
