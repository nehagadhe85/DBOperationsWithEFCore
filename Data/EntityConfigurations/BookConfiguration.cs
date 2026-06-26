using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DBOperationWithEFCore.Models;

namespace DBOperationWithEFCore.Data.EntityConfigurations;

/// <summary>
/// Fluent API configuration for the Book entity.
/// Demonstrates: IEntityTypeConfiguration pattern for clean, separated configuration.
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // Table name
        builder.ToTable("Books");

        // Primary key
        builder.HasKey(b => b.Id);

        // Property configurations
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(13);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        // Index on ISBN for fast lookups (unique constraint)
        builder.HasIndex(b => b.ISBN)
            .IsUnique()
            .HasDatabaseName("IX_Books_ISBN");

        // Relationship: Book belongs to one Author
        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        // Relationship: Book belongs to one Category
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter — Soft Delete
        // All queries will automatically exclude soft-deleted books
        // Use .IgnoreQueryFilters() to bypass when needed
        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}
