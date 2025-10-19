using FluentAssertions;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Tests.Utils;

namespace PaperlessRESTAPI.Tests.Repositories;

/// <summary>
/// Unit tests for DocumentRepository
/// </summary>
public class DocumentRepositoryTests : TestBase
{
    private readonly DocumentRepository _repository;

    public DocumentRepositoryTests()
    {
        _repository = new DocumentRepository(Context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocuments()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var documents = await _repository.GetAllAsync();

        // Assert
        documents.Should().HaveCount(2);
        documents.Should().OnlyContain(d => d.Tags != null);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnDocument()
    {
        // Arrange
        await SeedTestDataAsync();
        var firstDoc = Context.Documents.First();

        // Act
        var document = await _repository.GetByIdAsync(firstDoc.Id);

        // Assert
        document.Should().NotBeNull();
        document!.Id.Should().Be(firstDoc.Id);
        document.Title.Should().Be(firstDoc.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var document = await _repository.GetByIdAsync(999);

        // Assert
        document.Should().BeNull();
    }

    [Fact]
    public async Task SearchByTextAsync_WithMatchingTitle_ShouldReturnDocuments()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var documents = await _repository.SearchByTextAsync("First");

        // Assert
        documents.Should().HaveCount(1);
        documents.First().Title.Should().Contain("First");
    }

    [Fact]
    public async Task SearchByTextAsync_WithMatchingOcrText_ShouldReturnDocuments()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var documents = await _repository.SearchByTextAsync("OCR");

        // Assert
        documents.Should().HaveCount(1);
        documents.First().OcrText.Should().Contain("OCR");
    }

    [Fact]
    public async Task SearchByTextAsync_WithEmptyQuery_ShouldReturnAllDocuments()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var documents = await _repository.SearchByTextAsync("");

        // Assert
        documents.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByFileNameAsync_WithValidFileName_ShouldReturnDocument()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var document = await _repository.GetByFileNameAsync("first.pdf");

        // Assert
        document.Should().NotBeNull();
        document!.FileName.Should().Be("first.pdf");
    }

    [Fact]
    public async Task GetByFileNameAsync_WithInvalidFileName_ShouldReturnNull()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var document = await _repository.GetByFileNameAsync("nonexistent.pdf");

        // Assert
        document.Should().BeNull();
    }

    [Fact]
    public async Task GetRecentDocumentsAsync_ShouldReturnDocumentsOrderedByUploadDate()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var documents = await _repository.GetRecentDocumentsAsync(1);

        // Assert
        documents.Should().HaveCount(1);
        var recentDoc = documents.First();
        recentDoc.UploadDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocumentToDatabase()
    {
        // Arrange
        var document = CreateTestDocument("New Document", "new.pdf");

        // Act
        await _repository.AddAsync(document);
        await _repository.SaveChangesAsync();

        // Assert
        var savedDocument = await _repository.GetByIdAsync(document.Id);
        savedDocument.Should().NotBeNull();
        savedDocument!.Title.Should().Be("New Document");
    }

    [Fact]
    public async Task Update_ShouldModifyExistingDocument()
    {
        // Arrange
        await SeedTestDataAsync();
        var document = await _repository.GetByIdAsync(Context.Documents.First().Id);
        var newTitle = "Updated Title";

        // Act
        document!.Title = newTitle;
        _repository.Update(document);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedDocument = await _repository.GetByIdAsync(document.Id);
        updatedDocument!.Title.Should().Be(newTitle);
    }

    [Fact]
    public async Task Delete_ShouldRemoveDocumentFromDatabase()
    {
        // Arrange
        await SeedTestDataAsync();
        var document = Context.Documents.First();
        var documentId = document.Id;

        // Act
        _repository.Delete(document);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedDocument = await _repository.GetByIdAsync(documentId);
        deletedDocument.Should().BeNull();
    }

    [Fact]
    public async Task GetUnprocessedDocumentsAsync_ShouldReturnOnlyUnprocessedDocuments()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var unprocessedDocs = await _repository.GetUnprocessedDocumentsAsync();

        // Assert
        unprocessedDocs.Should().HaveCount(1);
        unprocessedDocs.Should().OnlyContain(d => !d.IsProcessed);
    }
}
