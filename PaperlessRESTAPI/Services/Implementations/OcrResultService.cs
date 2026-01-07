using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Services.Implementations;

/// <summary>
/// Service for processing OCR results and updating documents
/// </summary>
public class OcrResultService : IOcrResultService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OcrResultService> _logger;

    public OcrResultService(IUnitOfWork unitOfWork, ILogger<OcrResultService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> ProcessOcrResultAsync(OcrResultMessage ocrResult)
    {
        try
        {
            _logger.LogInformation("Processing OCR result for document {DocumentId}", ocrResult.DocumentId);

            var document = await _unitOfWork.Documents.GetByIdAsync(ocrResult.DocumentId);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for OCR result processing", ocrResult.DocumentId);
                return false;
            }

            // Update document with OCR results
            if (ocrResult.Success && !string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
            {
                document.Content = ocrResult.ExtractedText;
                document.IsProcessed = true;
                
                _logger.LogInformation("Updated document {DocumentId} with OCR text ({TextLength} characters, confidence: {Confidence:F2})",
                    ocrResult.DocumentId, ocrResult.ExtractedText.Length, ocrResult.Confidence);
            }
            else
            {
                document.IsProcessed = true; // Mark as processed even if failed
                _logger.LogWarning("OCR processing failed for document {DocumentId}: {ErrorMessage}", 
                    ocrResult.DocumentId, ocrResult.ErrorMessage);
            }

            document.LastModified = DateTime.UtcNow;

            _unitOfWork.Documents.Update(document);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully processed OCR result for document {DocumentId}", ocrResult.DocumentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OCR result for document {DocumentId}", ocrResult.DocumentId);
            return false;
        }
    }

    public async Task<OcrProcessingStatus> GetOcrProcessingStatusAsync(int documentId)
    {
        try
        {
            var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for status check", documentId);
                return new OcrProcessingStatus
                {
                    DocumentId = documentId,
                    IsProcessed = false,
                    HasOcrText = false
                };
            }

            return new OcrProcessingStatus
            {
                DocumentId = documentId,
                IsProcessed = document.IsProcessed,
                HasOcrText = !string.IsNullOrWhiteSpace(document.Content),
                ProcessedAt = document.LastModified
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting OCR processing status for document {DocumentId}", documentId);
            return new OcrProcessingStatus
            {
                DocumentId = documentId,
                IsProcessed = false,
                HasOcrText = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
