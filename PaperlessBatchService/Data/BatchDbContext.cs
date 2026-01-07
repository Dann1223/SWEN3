using Microsoft.EntityFrameworkCore;
using PaperlessBatchService.Data.Entities;

namespace PaperlessBatchService.Data;

public class BatchDbContext : DbContext
{
    public BatchDbContext(DbContextOptions<BatchDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DailyDocumentAccess> DailyDocumentAccess { get; set; }
    public DbSet<BatchProcessingHistory> BatchProcessingHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DailyDocumentAccess
        modelBuilder.Entity<DailyDocumentAccess>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentId, e.AccessDate })
                  .IsUnique()
                  .HasDatabaseName("IX_DailyDocumentAccess_DocumentId_AccessDate");

            entity.HasOne(d => d.Document)
                  .WithMany(p => p.DailyAccess)
                  .HasForeignKey(d => d.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure BatchProcessingHistory
        modelBuilder.Entity<BatchProcessingHistory>(entity =>
        {
            entity.HasIndex(e => e.FileName)
                  .HasDatabaseName("IX_BatchProcessingHistory_FileName");

            entity.HasIndex(e => e.ProcessedAt)
                  .HasDatabaseName("IX_BatchProcessingHistory_ProcessedAt");
        });
    }
}
