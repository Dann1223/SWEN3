using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using PaperlessRESTAPI.Configuration;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Services.Implementations;

/// <summary>
/// Service implementation for processing daily access log XML files
/// </summary>
public class BatchProcessingService : IBatchProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchProcessingService> _logger;
    private readonly BatchProcessingOptions _options;

    public BatchProcessingService(
        IUnitOfWork unitOfWork,
        ILogger<BatchProcessingService> logger,
        IOptions<BatchProcessingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _options = options.Value;
    }

    public async Task ProcessAccessLogFilesAsync()
    {
        try
        {
            _logger.LogInformation("Starting batch processing of access log files in: {InputFolder}", _options.InputFolder);

            if (!Directory.Exists(_options.InputFolder))
            {
                _logger.LogWarning("Input folder does not exist: {InputFolder}", _options.InputFolder);
                return;
            }

            // Ensure archive and error folders exist
            Directory.CreateDirectory(_options.ArchiveFolder);
            Directory.CreateDirectory(_options.ErrorFolder);

            var files = Directory.GetFiles(_options.InputFolder, _options.FilePattern);
            _logger.LogInformation("Found {FileCount} files matching pattern: {Pattern}", files.Length, _options.FilePattern);

            var successCount = 0;
            var errorCount = 0;

            foreach (var filePath in files)
            {
                try
                {
                    var success = await ProcessSingleFileAsync(filePath);
                    if (success)
                    {
                        successCount++;
                        _logger.LogInformation("Successfully processed file: {FileName}", Path.GetFileName(filePath));
                    }
                    else
                    {
                        errorCount++;
                        _logger.LogError("Failed to process file: {FileName}", Path.GetFileName(filePath));
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "Unexpected error processing file: {FileName}", Path.GetFileName(filePath));
                    
                    // Move file to error folder
                    await ArchiveProcessedFileAsync(filePath, false);
                }
            }

            _logger.LogInformation("Batch processing completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during batch processing");
            throw;
        }
    }

    public async Task<bool> ProcessSingleFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileSize = new FileInfo(filePath).Length;
        var checksum = await CalculateFileChecksumAsync(filePath);

        try
        {
            // Check if file was already processed successfully
            if (await _unitOfWork.BatchProcessingHistories.IsFileProcessedAsync(fileName, checksum))
            {
                _logger.LogInformation("File already processed successfully, skipping: {FileName}", fileName);
                await ArchiveProcessedFileAsync(filePath, true);
                return true;
            }

            // Parse XML file
            var accessLogReport = await ParseXmlFileAsync(filePath);
            if (accessLogReport == null)
            {
                await RecordProcessingHistoryAsync(fileName, filePath, false, "Failed to parse XML", 0, fileSize, checksum);
                await ArchiveProcessedFileAsync(filePath, false);
                return false;
            }

            // Validate date format
            if (!DateOnly.TryParse(accessLogReport.Date, out var reportDate))
            {
                var errorMsg = $"Invalid date format in XML: {accessLogReport.Date}";
                await RecordProcessingHistoryAsync(fileName, filePath, false, errorMsg, 0, fileSize, checksum);
                await ArchiveProcessedFileAsync(filePath, false);
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var recordsProcessed = 0;

                // Process each document access record
                foreach (var docAccess in accessLogReport.DocumentAccesses)
                {
                    // Verify document exists
                    var document = await _unitOfWork.Documents.GetByIdAsync(docAccess.DocumentId);
                    if (document == null)
                    {
                        _logger.LogWarning("Document not found for ID: {DocumentId}, skipping", docAccess.DocumentId);
                        continue;
                    }

                    // Create or update daily access record
                    var dailyAccess = new DailyDocumentAccess
                    {
                        DocumentId = docAccess.DocumentId,
                        AccessDate = reportDate,
                        ViewCount = docAccess.ViewCount,
                        DownloadCount = docAccess.DownloadCount,
                        SearchCount = docAccess.SearchCount,
                        TotalAccess = docAccess.TotalAccess
                    };

                    await _unitOfWork.DailyDocumentAccesses.UpsertDailyAccessAsync(dailyAccess);
                    recordsProcessed++;
                }

                await _unitOfWork.SaveChangesAsync();

                // Record successful processing
                await RecordProcessingHistoryAsync(fileName, filePath, true, null, recordsProcessed, fileSize, checksum);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Archive the file
                await ArchiveProcessedFileAsync(filePath, true);

                _logger.LogInformation("Successfully processed {RecordCount} records from file: {FileName}", 
                    recordsProcessed, fileName);

                return true;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {FileName}", fileName);
            await RecordProcessingHistoryAsync(fileName, filePath, false, ex.Message, 0, fileSize, checksum);
            await ArchiveProcessedFileAsync(filePath, false);
            return false;
        }
    }

    public async Task<AccessLogReportDto?> ParseXmlFileAsync(string filePath)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(AccessLogReportDto));
            
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var result = (AccessLogReportDto?)serializer.Deserialize(fileStream);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse XML file: {FilePath}", filePath);
            return null;
        }
    }

    public async Task ArchiveProcessedFileAsync(string filePath, bool isSuccessful)
    {
        var fileName = Path.GetFileName(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var archivedFileName = $"{timestamp}_{fileName}";
        
        var destinationFolder = isSuccessful ? _options.ArchiveFolder : _options.ErrorFolder;
        var destinationPath = Path.Combine(destinationFolder, archivedFileName);

        try
        {
            File.Move(filePath, destinationPath);
            _logger.LogInformation("File archived to: {DestinationPath}", destinationPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to archive file: {FilePath} to {DestinationPath}", filePath, destinationPath);
        }

        await Task.CompletedTask;
    }

    public async Task<string> CalculateFileChecksumAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        
        var hashBytes = await sha256.ComputeHashAsync(fileStream);
        return Convert.ToHexString(hashBytes);
    }

    private async Task RecordProcessingHistoryAsync(
        string fileName, 
        string filePath, 
        bool isSuccessful, 
        string? errorMessage, 
        int recordsProcessed, 
        long fileSizeBytes, 
        string checksum)
    {
        var history = new BatchProcessingHistory
        {
            FileName = fileName,
            FilePath = filePath,
            IsSuccessful = isSuccessful,
            ErrorMessage = errorMessage,
            RecordsProcessed = recordsProcessed,
            FileSizeBytes = fileSizeBytes,
            FileChecksum = checksum,
            ProcessedAt = DateTime.UtcNow
        };

        await _unitOfWork.BatchProcessingHistories.AddAsync(history);
    }
}
