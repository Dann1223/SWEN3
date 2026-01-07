using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Services.Interfaces;

/// <summary>
/// Service interface for processing daily access log XML files
/// </summary>
public interface IBatchProcessingService
{
    Task ProcessAccessLogFilesAsync();
    Task<bool> ProcessSingleFileAsync(string filePath);
    Task<AccessLogReportDto?> ParseXmlFileAsync(string filePath);
    Task ArchiveProcessedFileAsync(string filePath, bool isSuccessful);
    Task<string> CalculateFileChecksumAsync(string filePath);
}
