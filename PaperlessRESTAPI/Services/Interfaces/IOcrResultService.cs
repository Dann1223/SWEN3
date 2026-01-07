using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Services.Interfaces;

/// <summary>
/// Service for processing OCR results and updating documents
/// </summary>
public interface IOcrResultService
{
    /// <summary>
    /// Process OCR result and update document
    /// </summary>
    /// <param name="ocrResult">OCR result message</param>
    /// <returns>True if processing was successful</returns>
    Task<bool> ProcessOcrResultAsync(OcrResultMessage ocrResult);

    /// <summary>
    /// Get OCR processing status for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <returns>Processing status information</returns>
    Task<OcrProcessingStatus> GetOcrProcessingStatusAsync(int documentId);
}

/// <summary>
/// OCR processing status information
/// </summary>
public class OcrProcessingStatus
{
    public int DocumentId { get; set; }
    public bool IsProcessed { get; set; }
    public bool HasOcrText { get; set; }
    public float? Confidence { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessingMethod { get; set; }
    public string? ErrorMessage { get; set; }
}
