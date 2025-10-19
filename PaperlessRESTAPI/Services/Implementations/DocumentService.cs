using AutoMapper;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Infrastructure.Exceptions;
using PaperlessRESTAPI.Models.DTOs;
using PaperlessRESTAPI.Models.Messages;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Services.Implementations;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;
    private readonly IQueueService _queueService;
    private readonly IStorageService _storageService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IMapper mapper,
        IQueueService queueService,
        IStorageService storageService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
        _queueService = queueService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all documents");
            throw new DataException("Failed to retrieve documents");
        }
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(int id)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id);
            return document != null ? _mapper.Map<DocumentDto>(document) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document with ID {DocumentId}", id);
            throw new DataException($"Failed to retrieve document with ID {id}");
        }
    }

    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto)
    {
        try
        {
            var document = _mapper.Map<Document>(createDocumentDto);
            document.UploadDate = DateTime.UtcNow;
            document.LastModified = DateTime.UtcNow;

            var createdDocument = await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document created with ID {DocumentId}", createdDocument.Id);

            return _mapper.Map<DocumentDto>(createdDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            throw new BusinessException("DOC_CREATE_FAILED", "Failed to create document");
        }
    }

    public async Task<DocumentDto> UploadDocumentAsync(IFormFile file, string? title = null)
    {
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("INVALID_FILE", "No file provided or file is empty");
        }

        // Validate file type
        var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new BusinessException("INVALID_FILE_TYPE", 
                $"File type {fileExtension} is not supported. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Validate file size (10MB limit)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            throw new BusinessException("FILE_TOO_LARGE", "File size cannot exceed 10MB");
        }

        try
        {
            // Upload file to MinIO
            using var stream = file.OpenReadStream();
            var storagePath = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType);

            // Create document entity
            var document = new Document
            {
                Title = title ?? Path.GetFileNameWithoutExtension(file.FileName),
                FileName = file.FileName,
                FilePath = storagePath, // MinIO storage path
                FileType = fileExtension,
                FileSize = file.Length,
                UploadDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsProcessed = false,
                IsIndexed = false
            };

            var createdDocument = await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document uploaded with ID {DocumentId}, FileName: {FileName}, StoragePath: {StoragePath}", 
                createdDocument.Id, file.FileName, storagePath);

            // Send OCR message to queue
            var ocrMessage = new OcrMessage
            {
                DocumentId = createdDocument.Id,
                FileName = file.FileName,
                FilePath = storagePath, // MinIO storage path
                FileType = fileExtension,
                RequestedAt = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await _queueService.SendOcrMessageAsync(ocrMessage);

            _logger.LogInformation("OCR message sent for document {DocumentId}, CorrelationId: {CorrelationId}", 
                createdDocument.Id, ocrMessage.CorrelationId);

            return _mapper.Map<DocumentDto>(createdDocument);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName}", file.FileName);
            throw new BusinessException("UPLOAD_FAILED", "Failed to upload document");
        }
    }

    public async Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return null;
            }

            _mapper.Map(updateDocumentDto, document);
            document.LastModified = DateTime.UtcNow;

            // Update using the base repository method
            _documentRepository.Update(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} updated", id);

            return _mapper.Map<DocumentDto>(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", id);
            throw new DataException($"Failed to update document with ID {id}");
        }
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return false;
            }

            // Delete file from MinIO storage
            try
            {
                await _storageService.DeleteFileAsync(document.FilePath);
                _logger.LogInformation("File deleted from storage: {FilePath}", document.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file from storage: {FilePath}", document.FilePath);
                // Continue with database deletion even if storage deletion fails
            }

            _documentRepository.Delete(document);
            await _documentRepository.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} deleted", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            throw new DataException($"Failed to delete document with ID {id}");
        }
    }

    public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm)
    {
        try
        {
            var documents = await _documentRepository.SearchByTextAsync(searchTerm);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents with term {SearchTerm}", searchTerm);
            throw new DataException("Failed to search documents");
        }
    }

    public async Task<IEnumerable<DocumentDto>> GetRecentDocumentsAsync(int count = 10)
    {
        try
        {
            var documents = await _documentRepository.GetRecentDocumentsAsync(count);
            return _mapper.Map<IEnumerable<DocumentDto>>(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            throw new DataException("Failed to retrieve recent documents");
        }
    }
}
