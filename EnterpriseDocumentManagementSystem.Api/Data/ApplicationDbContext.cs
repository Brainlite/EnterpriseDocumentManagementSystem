using Microsoft.EntityFrameworkCore;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentShare> DocumentShares { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FileSize)
                .IsRequired();

            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.FilePath)
                .HasMaxLength(500);

            entity.Property(e => e.AccessType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.UploadedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastModifiedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            // Indexes
            entity.HasIndex(e => e.UploadedBy)
                .HasDatabaseName("IX_Documents_UploadedBy");

            entity.HasIndex(e => e.CreatedDate)
                .HasDatabaseName("IX_Documents_CreatedDate");

            entity.HasIndex(e => e.AccessType)
                .HasDatabaseName("IX_Documents_AccessType");

            entity.HasIndex(e => e.IsDeleted)
                .HasDatabaseName("IX_Documents_IsDeleted");

            entity.HasIndex(e => new { e.UploadedBy, e.IsDeleted })
                .HasDatabaseName("IX_Documents_UploadedBy_IsDeleted");
        });

        // DocumentShare configuration
        modelBuilder.Entity<DocumentShare>(entity =>
        {
            entity.ToTable("DocumentShares");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.DocumentId)
                .IsRequired();

            entity.Property(e => e.SharedWithUserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PermissionLevel)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.SharedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.SharedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsRevoked)
                .HasDefaultValue(false);

            entity.Property(e => e.RevokedBy)
                .HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Document)
                .WithMany(d => d.DocumentShares)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("IX_DocumentShares_DocumentId");

            entity.HasIndex(e => e.SharedWithUserId)
                .HasDatabaseName("IX_DocumentShares_SharedWithUserId");

            entity.HasIndex(e => new { e.DocumentId, e.SharedWithUserId, e.IsRevoked })
                .HasDatabaseName("IX_DocumentShares_DocumentId_SharedWithUserId_IsRevoked");
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Color)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            // Unique constraint on tag name
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("IX_Tags_Name_Unique");
        });

        // DocumentTag configuration
        modelBuilder.Entity<DocumentTag>(entity =>
        {
            entity.ToTable("DocumentTags");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.DocumentId)
                .IsRequired();

            entity.Property(e => e.TagId)
                .IsRequired();

            entity.Property(e => e.AssignedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.AssignedBy)
                .IsRequired()
                .HasMaxLength(100);

            // Relationships
            entity.HasOne(e => e.Document)
                .WithMany(d => d.DocumentTags)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.DocumentTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("IX_DocumentTags_DocumentId");

            entity.HasIndex(e => e.TagId)
                .HasDatabaseName("IX_DocumentTags_TagId");

            entity.HasIndex(e => new { e.DocumentId, e.TagId })
                .IsUnique()
                .HasDatabaseName("IX_DocumentTags_DocumentId_TagId_Unique");
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ActionType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.EntityType)
                .HasMaxLength(500);

            entity.Property(e => e.Details)
                .HasMaxLength(2000);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.Timestamp)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IsSuccessful)
                .HasDefaultValue(true);

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000);

            // Relationships
            entity.HasOne(e => e.Document)
                .WithMany(d => d.AuditLogs)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.DocumentId)
                .HasDatabaseName("IX_AuditLogs_DocumentId");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_AuditLogs_Timestamp");

            entity.HasIndex(e => e.ActionType)
                .HasDatabaseName("IX_AuditLogs_ActionType");

            entity.HasIndex(e => new { e.UserId, e.Timestamp })
                .HasDatabaseName("IX_AuditLogs_UserId_Timestamp");
        });
    }
}
