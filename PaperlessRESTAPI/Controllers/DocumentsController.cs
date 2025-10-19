using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Controllers;

/// <summary>
/// Documents API Controller for managing document operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDocumentService _documentService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IUnitOfWork unitOfWork,
        IDocumentService documentService,
        IMapper mapper,
        ILogger<DocumentsController> logger)
    {
        _unitOfWork = unitOfWork;
        _documentService = documentService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetDocuments()
    {
        try
        {
            _logger.LogInformation("Retrieving all documents");
            
            var documents = await _unitOfWork.Documents.GetAllAsync();
            var documentDtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
            
            return Ok(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving document with ID: {DocumentId}", id);
            
            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found");
            }

            var documentDto = _mapper.Map<DocumentDto>(document);
            return Ok(documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document with ID: {DocumentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Upload a new document
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> UploadDocument([FromForm] UploadDocumentDto uploadDto)
    {
        try
        {
            _logger.LogInformation("Uploading document: {FileName}", uploadDto.File.FileName);

            var documentDto = await _documentService.UploadDocumentAsync(
                uploadDto.File, 
                uploadDto.Title);

            return CreatedAtAction(nameof(GetDocument), new { id = documentDto.Id }, documentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", uploadDto.File.FileName);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, [FromBody] DocumentDto documentDto)
    {
        try
        {
            _logger.LogInformation("Updating document with ID: {DocumentId}", id);

            var existingDocument = await _unitOfWork.Documents.GetByIdAsync(id);
            
            if (existingDocument == null)
            {
                return NotFound($"Document with ID {id} not found");
            }

            // Update fields (excluding file-related fields)
            existingDocument.Title = documentDto.Title;
            existingDocument.LastModified = DateTime.UtcNow;

            _unitOfWork.Documents.Update(existingDocument);
            await _unitOfWork.SaveChangesAsync();

            var updatedDocumentDto = _mapper.Map<DocumentDto>(existingDocument);
            return Ok(updatedDocumentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document with ID: {DocumentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete document
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(int id)
    {
        try
        {
            _logger.LogInformation("Deleting document with ID: {DocumentId}", id);

            var document = await _unitOfWork.Documents.GetByIdAsync(id);
            
            if (document == null)
            {
                return NotFound($"Document with ID {id} not found");
            }

            // TODO: Delete physical file from storage
            
            _unitOfWork.Documents.Delete(document);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Document deleted successfully with ID: {DocumentId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document with ID: {DocumentId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Search documents
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<SearchResultDto>> SearchDocuments([FromQuery] string query = "")
    {
        try
        {
            _logger.LogInformation("Searching documents with query: {SearchQuery}", query);

            var startTime = DateTime.UtcNow;
            var documents = await _unitOfWork.Documents.SearchByTextAsync(query);
            var searchDuration = DateTime.UtcNow - startTime;

            var documentDtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
            
            var searchResult = new SearchResultDto
            {
                Documents = documentDtos.ToList(),
                TotalCount = documentDtos.Count(),
                SearchTerm = query,
                SearchDuration = searchDuration
            };

            return Ok(searchResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents with query: {SearchQuery}", query);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get recent documents
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetRecentDocuments([FromQuery] int count = 10)
    {
        try
        {
            _logger.LogInformation("Retrieving {Count} recent documents", count);
            
            var documents = await _unitOfWork.Documents.GetRecentDocumentsAsync(count);
            var documentDtos = _mapper.Map<IEnumerable<DocumentDto>>(documents);
            
            return Ok(documentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            return StatusCode(500, "Internal server error");
        }
    }
}
