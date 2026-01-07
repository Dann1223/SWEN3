using PaperlessServices.Services.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SkiaSharp;
using PDFtoImage;

namespace PaperlessServices.Services.Implementations;

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly IOcrService _ocrService;
    private readonly ILogger<DocumentProcessingService> _logger;

    // Supported file types
    private static readonly HashSet<string> PdfExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf" };
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase) 
    { 
        ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".gif", ".webp" 
    };

    public DocumentProcessingService(IOcrService ocrService, ILogger<DocumentProcessingService> logger)
    {
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(Stream documentStream, string fileName, string language = "eng")
    {
        var fileExtension = Path.GetExtension(fileName);
        _logger.LogInformation("Processing document: {FileName} with extension: {Extension}", fileName, fileExtension);

        try
        {
            if (PdfExtensions.Contains(fileExtension))
            {
                return await ProcessPdfDocumentAsync(documentStream, fileName, language);
            }
            else if (ImageExtensions.Contains(fileExtension))
            {
                return await ProcessImageDocumentAsync(documentStream, fileName, language);
            }
            else
            {
                _logger.LogWarning("Unsupported file type: {Extension} for file: {FileName}", fileExtension, fileName);
                return $"[Unsupported file type: {fileExtension}. Please upload PDF or image files.]";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process document: {FileName}", fileName);
            return $"[Error processing document: {ex.Message}]";
        }
    }

    public bool CanProcess(string fileName)
    {
        var fileExtension = Path.GetExtension(fileName);
        return PdfExtensions.Contains(fileExtension) || ImageExtensions.Contains(fileExtension);
    }

    public string GetProcessingMethod(string fileName)
    {
        var fileExtension = Path.GetExtension(fileName);
        
        if (PdfExtensions.Contains(fileExtension))
        {
            return "PDF Text Extraction + OCR Fallback";
        }
        else if (ImageExtensions.Contains(fileExtension))
        {
            return "Tesseract OCR";
        }
        else
        {
            return "Unsupported";
        }
    }

    private async Task<string> ProcessPdfDocumentAsync(Stream pdfStream, string fileName, string language)
    {
        _logger.LogInformation("Processing PDF document: {FileName}", fileName);

        // First, try to extract text directly from PDF
        try
        {
            var directText = await ExtractTextDirectlyFromPdfAsync(pdfStream);
            if (!string.IsNullOrWhiteSpace(directText) && directText.Trim().Length > 50)
            {
                _logger.LogInformation("Successfully extracted text directly from PDF: {FileName}, length: {Length}", 
                    fileName, directText.Length);
                return directText;
            }
            
            _logger.LogInformation("PDF appears to be image-based or has minimal text. Text length: {Length}", 
                directText?.Length ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Direct PDF text extraction failed for: {FileName}. Falling back to OCR.", fileName);
        }

        // Fallback: Convert PDF pages to images and use OCR
        try
        {
            var ocrText = await ExtractTextFromPdfWithOcrAsync(pdfStream, fileName, language);
            _logger.LogInformation("OCR extraction completed for PDF: {FileName}, length: {Length}", 
                fileName, ocrText.Length);
            return ocrText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR fallback failed for PDF: {FileName}", fileName);
            return $"[PDF processing failed: {ex.Message}. This may be a scanned document that requires advanced OCR processing.]";
        }
    }

    private async Task<string> ProcessImageDocumentAsync(Stream imageStream, string fileName, string language)
    {
        _logger.LogInformation("Processing image document with OCR: {FileName}", fileName);
        
        try
        {
            var extractedText = await _ocrService.ExtractTextAsync(imageStream, language);
            _logger.LogInformation("OCR extraction completed for image: {FileName}, length: {Length}", 
                fileName, extractedText.Length);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed for image: {FileName}", fileName);
            return $"[Image OCR processing failed: {ex.Message}]";
        }
    }

    private async Task<string> ExtractTextDirectlyFromPdfAsync(Stream pdfStream)
    {
        // Reset stream position
        pdfStream.Position = 0;
        
        using var pdfReader = new PdfReader(pdfStream);
        using var pdfDocument = new PdfDocument(pdfReader);
        
        var text = new List<string>();
        var strategy = new SimpleTextExtractionStrategy();
        
        for (int pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
        {
            var page = pdfDocument.GetPage(pageNum);
            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
            
            if (!string.IsNullOrWhiteSpace(pageText))
            {
                text.Add(pageText.Trim());
            }
        }
        
        var combinedText = string.Join("\n\n", text);
        return await Task.FromResult(combinedText);
    }

    private async Task<string> ExtractTextFromPdfWithOcrAsync(Stream pdfStream, string fileName, string language)
    {
        _logger.LogInformation("Starting PDF OCR processing for: {FileName}", fileName);
        
        try
        {
            // Create a copy of the stream to avoid "Cannot access a closed Stream" errors
            using var memoryStream = new MemoryStream();
            pdfStream.Position = 0;
            await pdfStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            var extractedTexts = new List<string>();
            
            // Convert PDF pages to images using PDFtoImage with the copied stream
            var images = Conversion.ToImages(memoryStream);
            var imageList = images.ToList(); // Materialize the enumerable to avoid multiple enumerations
            var pageCount = imageList.Count;
            
            _logger.LogInformation("Processing {PageCount} pages from PDF: {FileName}", pageCount, fileName);
            
            for (int pageNum = 0; pageNum < pageCount; pageNum++)
            {
                SKBitmap? image = null;
                try
                {
                    image = imageList[pageNum];
                    _logger.LogDebug("Processing page {PageNum} of {PageCount} for: {FileName}", pageNum + 1, pageCount, fileName);
                    
                    // Convert SkiaSharp image to stream for OCR
                    using var imageStream = new MemoryStream();
                    using var skImage = SKImage.FromBitmap(image);
                    using var data = skImage.Encode(SKEncodedImageFormat.Png, 85);
                    data.SaveTo(imageStream);
                    imageStream.Position = 0;
                    
                    // Perform OCR on the page image
                    var pageText = await _ocrService.ExtractTextAsync(imageStream, language);
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        extractedTexts.Add($"--- Page {pageNum + 1} ---\n{pageText.Trim()}");
                    }
                    else
                    {
                        extractedTexts.Add($"--- Page {pageNum + 1} ---\n[No text detected on this page]");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process page {PageNum} of PDF: {FileName}", pageNum + 1, fileName);
                    extractedTexts.Add($"--- Page {pageNum + 1} ---\n[Error processing page: {ex.Message}]");
                }
                finally
                {
                    image?.Dispose();
                }
            }
            
            var combinedText = string.Join("\n\n", extractedTexts);
            _logger.LogInformation("PDF OCR processing completed for: {FileName}, extracted {CharCount} characters from {PageCount} pages", 
                fileName, combinedText.Length, pageCount);
            
            return combinedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF OCR processing failed for: {FileName}", fileName);
            return $"[PDF OCR Processing Failed]\n" +
                   $"File: {fileName}\n" +
                   $"Error: {ex.Message}\n" +
                   $"This document could not be processed. Please ensure it's a valid PDF file.";
        }
    }
}
