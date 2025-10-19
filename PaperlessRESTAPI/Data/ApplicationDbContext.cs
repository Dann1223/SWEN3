using Microsoft.EntityFrameworkCore;
using PaperlessRESTAPI.Data.Entities;

namespace PaperlessRESTAPI.Data;

/// <summary>
/// Application database context for the Document Management System
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Document> Documents { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DocumentAccess> DocumentAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.HasIndex(e => e.FileName).IsUnique();
            entity.HasIndex(e => e.UploadDate);
            entity.HasIndex(e => e.IsProcessed);
            entity.HasIndex(e => e.IsIndexed);

            // Configure many-to-many relationship with Tags
            entity.HasMany(d => d.Tags)
                  .WithMany(t => t.Documents)
                  .UsingEntity<Dictionary<string, object>>(
                      "DocumentTag",
                      j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                      j => j.HasOne<Document>().WithMany().HasForeignKey("DocumentId"),
                      j =>
                      {
                          j.HasKey("DocumentId", "TagId");
                          j.ToTable("DocumentTags");
                      });

            // Configure one-to-many relationship with DocumentAccess
            entity.HasMany(d => d.AccessLogs)
                  .WithOne(a => a.Document)
                  .HasForeignKey(a => a.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Color).HasDefaultValue("#007bff");
        });

        // Configure DocumentAccess entity
        modelBuilder.Entity<DocumentAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.AccessDate);
            entity.HasIndex(e => e.ActionType);
        });

        // Seed default tags
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Important", Description = "Important documents", Color = "#dc3545" },
            new Tag { Id = 2, Name = "Archive", Description = "Archived documents", Color = "#6c757d" },
            new Tag { Id = 3, Name = "Invoice", Description = "Invoice documents", Color = "#28a745" },
            new Tag { Id = 4, Name = "Contract", Description = "Contract documents", Color = "#007bff" },
            new Tag { Id = 5, Name = "Report", Description = "Report documents", Color = "#fd7e14" }
        );
    }
}
