using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data;
using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Tests.Utils;

/// <summary>
/// Base class for tests providing common setup methods
/// </summary>
public abstract class TestBase : IDisposable
{
    protected ApplicationDbContext Context { get; private set; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Create a test document entity
    /// </summary>
    protected Document CreateTestDocument(string title = "Test Document", string fileName = "test.pdf")
    {
        return new Document
        {
            Title = title,
            FileName = fileName,
            FilePath = $"/uploads/{fileName}",
            FileType = ".pdf",
            FileSize = 1024,
            UploadDate = DateTime.UtcNow,
            IsProcessed = false,
            IsIndexed = false
        };
    }

    /// <summary>
    /// Create a test tag entity
    /// </summary>
    protected Tag CreateTestTag(string name = "Test Tag", string color = "#007bff")
    {
        return new Tag
        {
            Name = name,
            Color = color,
            Description = $"Description for {name}",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Seed the context with test data
    /// </summary>
    protected async Task SeedTestDataAsync()
    {
        var tag1 = CreateTestTag("Important", "#dc3545");
        var tag2 = CreateTestTag("Archive", "#6c757d");

        Context.Tags.AddRange(tag1, tag2);

        var doc1 = CreateTestDocument("First Document", "first.pdf");
        var doc2 = CreateTestDocument("Second Document", "second.pdf");
        doc2.OcrText = "This is OCR text content";
        doc2.Summary = "This is a summary";
        doc2.IsProcessed = true;

        Context.Documents.AddRange(doc1, doc2);

        await Context.SaveChangesAsync();

        // Add tags to documents
        doc1.Tags.Add(tag1);
        doc2.Tags.Add(tag2);
        
        await Context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }
}
