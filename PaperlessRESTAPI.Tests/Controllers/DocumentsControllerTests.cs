using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PaperlessRESTAPI.Controllers;
using PaperlessRESTAPI.Data.Entities;
using PaperlessRESTAPI.Data.Mapping;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Tests.Controllers;

/// <summary>
/// Unit tests for DocumentsController
/// </summary>
public class DocumentsControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IRepository<Tag>> _mockTagRepository;
    private readonly Mock<ILogger<DocumentsController>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly DocumentsController _controller;

    public DocumentsControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockTagRepository = new Mock<IRepository<Tag>>();
        _mockLogger = new Mock<ILogger<DocumentsController>>();

        // Setup AutoMapper
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        _mapper = configuration.CreateMapper();

        // Setup mock unit of work
        _mockUnitOfWork.Setup(u => u.Documents).Returns(_mockDocumentRepository.Object);
        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepository.Object);

        _controller = new DocumentsController(_mockUnitOfWork.Object, _mapper, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDocuments_ShouldReturnAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            CreateTestDocument(1, "Document 1", "doc1.pdf"),
            CreateTestDocument(2, "Document 2", "doc2.pdf")
        };

        _mockDocumentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetDocuments();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedDocuments = okResult!.Value as IEnumerable<DocumentDto>;
        returnedDocuments.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDocument_WithValidId_ShouldReturnDocument()
    {
        // Arrange
        var documentId = 1;
        var document = CreateTestDocument(documentId, "Test Document", "test.pdf");

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedDocument = okResult!.Value as DocumentDto;
        returnedDocument!.Id.Should().Be(documentId);
        returnedDocument.Title.Should().Be("Test Document");
    }

    [Fact]
    public async Task GetDocument_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var documentId = 999;
        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.GetDocument(documentId);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateDocument_WithValidData_ShouldReturnUpdatedDocument()
    {
        // Arrange
        var documentId = 1;
        var existingDocument = CreateTestDocument(documentId, "Old Title", "test.pdf");
        var updateDto = new DocumentDto
        {
            Id = documentId,
            Title = "New Title",
            FileName = "test.pdf",
            FileType = ".pdf",
            FileSize = 1024,
            UploadDate = DateTime.UtcNow
        };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync(existingDocument);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.UpdateDocument(documentId, updateDto);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedDocument = okResult!.Value as DocumentDto;
        returnedDocument!.Title.Should().Be("New Title");

        _mockDocumentRepository.Verify(r => r.Update(It.IsAny<Document>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateDocument_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var documentId = 999;
        var updateDto = new DocumentDto { Id = documentId, Title = "New Title" };

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.UpdateDocument(documentId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteDocument_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var documentId = 1;
        var document = CreateTestDocument(documentId, "Test Document", "test.pdf");

        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync(document);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockDocumentRepository.Verify(r => r.Delete(It.IsAny<Document>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteDocument_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var documentId = 999;
        _mockDocumentRepository.Setup(r => r.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _controller.DeleteDocument(documentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SearchDocuments_ShouldReturnSearchResults()
    {
        // Arrange
        var searchQuery = "test";
        var documents = new List<Document>
        {
            CreateTestDocument(1, "Test Document", "test.pdf")
        };

        _mockDocumentRepository.Setup(r => r.SearchByTextAsync(searchQuery))
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.SearchDocuments(searchQuery);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var searchResult = okResult!.Value as SearchResultDto;
        searchResult!.Documents.Should().HaveCount(1);
        searchResult.TotalCount.Should().Be(1);
        searchResult.SearchTerm.Should().Be(searchQuery);
    }

    [Fact]
    public async Task GetRecentDocuments_ShouldReturnRecentDocuments()
    {
        // Arrange
        var count = 5;
        var documents = new List<Document>
        {
            CreateTestDocument(1, "Recent Document 1", "recent1.pdf"),
            CreateTestDocument(2, "Recent Document 2", "recent2.pdf")
        };

        _mockDocumentRepository.Setup(r => r.GetRecentDocumentsAsync(count))
            .ReturnsAsync(documents);

        // Act
        var result = await _controller.GetRecentDocuments(count);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedDocuments = okResult!.Value as IEnumerable<DocumentDto>;
        returnedDocuments.Should().HaveCount(2);
    }

    #region Helper Methods

    private static Document CreateTestDocument(int id, string title, string fileName)
    {
        return new Document
        {
            Id = id,
            Title = title,
            FileName = fileName,
            FilePath = $"/uploads/{fileName}",
            FileType = Path.GetExtension(fileName),
            FileSize = 1024,
            UploadDate = DateTime.UtcNow,
            IsProcessed = false,
            IsIndexed = false,
            Tags = new List<Tag>()
        };
    }

    #endregion
}
