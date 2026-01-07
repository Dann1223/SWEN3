using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using PaperlessBatchService.Configuration;
using PaperlessBatchService.Data;
using PaperlessBatchService.Data.Entities;
using PaperlessBatchService.Models.DTOs;

namespace PaperlessBatchService.Services;

public interface IBatchProcessingService
{
    Task ProcessAccessLogFilesAsync();
}

public class BatchProcessingService : IBatchProcessingService
{
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly BatchDbContext _context;
    private readonly BatchProcessingOptions _options;

    public BatchProcessingService(
        ILogger<BatchProcessingService> logger,
        BatchDbContext context,
        IOptions<BatchProcessingOptions> options)
    {
        _logger = logger;
        _context = context;
        _options = options.Value;
    }

    public async Task ProcessAccessLogFilesAsync()
    {
        _logger.LogInformation("Starting batch processing of access log files");

        if (!_options.IsEnabled)
        {
            _logger.LogInformation("Batch processing is disabled");
            return;
        }

        try
        {
            // Ensure directories exist
            EnsureDirectoriesExist();

            // Get XML files to process
            var files = GetFilesToProcess();
            _logger.LogInformation("Found {FileCount} files to process", files.Count);

            var processedCount = 0;
            var errorCount = 0;

            foreach (var file in files.Take(_options.MaxFilesPerBatch))
            {
                try
                {
                    await ProcessSingleFileAsync(file);
                    processedCount++;
                    _logger.LogDebug("Successfully processed file: {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "Failed to process file: {FileName}", Path.GetFileName(file));
                    await MoveFileToErrorFolder(file, ex.Message);
                }
            }

            _logger.LogInformation("Batch processing completed. Processed: {ProcessedCount}, Errors: {ErrorCount}", 
                processedCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch processing");
            throw;
        }
    }

    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_options.InputFolder);
        Directory.CreateDirectory(_options.ArchiveFolder);
        Directory.CreateDirectory(_options.ErrorFolder);
    }

    private List<string> GetFilesToProcess()
    {
        if (!Directory.Exists(_options.InputFolder))
        {
            _logger.LogWarning("Input folder does not exist: {InputFolder}", _options.InputFolder);
            return new List<string>();
        }

        return Directory.GetFiles(_options.InputFolder, _options.FilePattern)
            .Where(file => new FileInfo(file).Length <= _options.MaxFileSizeBytes)
            .OrderBy(file => File.GetCreationTime(file))
            .ToList();
    }

    private async Task ProcessSingleFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new FileInfo(filePath);
        var checksum = CalculateFileChecksum(filePath);

        // Check if file already processed
        var existingRecord = await _context.BatchProcessingHistory
            .FirstOrDefaultAsync(h => h.FileName == fileName && h.FileChecksum == checksum);

        if (existingRecord != null)
        {
            _logger.LogInformation("File {FileName} already processed, skipping", fileName);
            await ArchiveProcessedFileAsync(filePath);
            return;
        }

        var history = new BatchProcessingHistory
        {
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileInfo.Length,
            FileChecksum = checksum,
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            // Parse XML file
            var accessLogReport = await ParseXmlFileAsync(filePath);
            
            // Validate date
            if (!DateOnly.TryParse(accessLogReport.Date, out var accessDate))
            {
                throw new InvalidOperationException($"Invalid date format in XML: {accessLogReport.Date}");
            }

            var recordsProcessed = 0;

            // Process each document access record
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            foreach (var docAccess in accessLogReport.DocumentAccesses)
            {
                // Check if document exists
                var documentExists = await _context.Documents
                    .AnyAsync(d => d.Id == docAccess.DocumentId);

                if (!documentExists)
                {
                    _logger.LogWarning("Document with ID {DocumentId} not found, skipping", docAccess.DocumentId);
                    continue;
                }

                // Check if record already exists for this date
                var existingAccess = await _context.DailyDocumentAccess
                    .FirstOrDefaultAsync(d => d.DocumentId == docAccess.DocumentId && d.AccessDate == accessDate);

                if (existingAccess != null)
                {
                    // Update existing record
                    existingAccess.ViewCount += docAccess.ViewCount;
                    existingAccess.DownloadCount += docAccess.DownloadCount;
                    existingAccess.SearchCount += docAccess.SearchCount;
                    existingAccess.TotalAccess += docAccess.TotalAccess;
                    existingAccess.UpdatedAt = DateTime.UtcNow;
                    
                    _context.DailyDocumentAccess.Update(existingAccess);
                }
                else
                {
                    // Create new record
                    var dailyAccess = new DailyDocumentAccess
                    {
                        DocumentId = docAccess.DocumentId,
                        AccessDate = accessDate,
                        ViewCount = docAccess.ViewCount,
                        DownloadCount = docAccess.DownloadCount,
                        SearchCount = docAccess.SearchCount,
                        TotalAccess = docAccess.TotalAccess,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.DailyDocumentAccess.AddAsync(dailyAccess);
                }

                recordsProcessed++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Mark as successful
            history.IsSuccessful = true;
            history.RecordsProcessed = recordsProcessed;

            _logger.LogInformation("Successfully processed {RecordsProcessed} records from file {FileName}", 
                recordsProcessed, fileName);

            // Archive the file
            await ArchiveProcessedFileAsync(filePath);
        }
        catch (Exception ex)
        {
            history.IsSuccessful = false;
            history.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            await _context.BatchProcessingHistory.AddAsync(history);
            await _context.SaveChangesAsync();
        }
    }

    private Task<AccessLogReportDto> ParseXmlFileAsync(string filePath)
    {
        var serializer = new XmlSerializer(typeof(AccessLogReportDto));
        
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var result = serializer.Deserialize(fileStream) as AccessLogReportDto;
        
        if (result == null)
        {
            throw new InvalidOperationException($"Failed to deserialize XML file: {filePath}");
        }

        return Task.FromResult(result);
    }

    private Task ArchiveProcessedFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var archivedFileName = $"{timestamp}_{fileName}";
        var archivePath = Path.Combine(_options.ArchiveFolder, archivedFileName);

        File.Move(filePath, archivePath);
        _logger.LogDebug("Archived file: {FileName} to {ArchivePath}", fileName, archivePath);
        
        return Task.CompletedTask;
    }

    private async Task MoveFileToErrorFolder(string filePath, string errorMessage)
    {
        try
        {
            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var errorFileName = $"{timestamp}_ERROR_{fileName}";
            var errorPath = Path.Combine(_options.ErrorFolder, errorFileName);

            File.Move(filePath, errorPath);
            
            // Create error log file
            var errorLogPath = Path.ChangeExtension(errorPath, ".error.txt");
            await File.WriteAllTextAsync(errorLogPath, 
                $"Error occurred at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                $"File: {fileName}\n" +
                $"Error: {errorMessage}\n");

            _logger.LogDebug("Moved failed file to error folder: {ErrorPath}", errorPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file to error folder: {FileName}", Path.GetFileName(filePath));
        }
    }

    private string CalculateFileChecksum(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(fileStream);
        return Convert.ToHexString(hashBytes);
    }
}
