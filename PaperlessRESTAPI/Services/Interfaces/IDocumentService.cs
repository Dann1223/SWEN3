using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Services.Interfaces;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();
    Task<DocumentDto?> GetDocumentByIdAsync(int id);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDocumentDto);
    Task<DocumentDto> UploadDocumentAsync(IFormFile file, string? title = null);
    Task<DocumentDto?> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto);
    Task<bool> DeleteDocumentAsync(int id);
    Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm);
    Task<IEnumerable<DocumentDto>> GetRecentDocumentsAsync(int count = 10);
}
