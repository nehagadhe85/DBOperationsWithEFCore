using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Data.EntityConfigurations;

namespace DBOperationWithEFCore.Data;

/// <summary>
/// EF Core DbContext for the Library Management System.
/// 
/// Key EF Core concepts demonstrated here:
/// - DbSet properties for each entity
/// - Fluent API configurations via IEntityTypeConfiguration
/// - Value Conversions (Enum → String)
/// - Unique indexes
/// - Seed Data (HasData)
/// - Global Query Filters (applied in BookConfiguration)
/// </summary>
public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    // DbSet properties — each represents a table in the database
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<BookLoan> BookLoans => Set<BookLoan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply the separate Book configuration class
        modelBuilder.ApplyConfiguration(new BookConfiguration());

        // ---------- Author Configuration ----------
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Authors");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.LastName).IsRequired().HasMaxLength(100);
        });

        // ---------- Category Configuration ----------
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

            // Unique index on Category Name
            entity.HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name");
        });

        // ---------- Member Configuration ----------
        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Members");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.FullName).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Email).IsRequired().HasMaxLength(250);

            // Unique index on Member Email
            entity.HasIndex(m => m.Email)
                .IsUnique()
                .HasDatabaseName("IX_Members_Email");
        });

        // ---------- BookLoan Configuration ----------
        modelBuilder.Entity<BookLoan>(entity =>
        {
            entity.ToTable("BookLoans");
            entity.HasKey(bl => bl.Id);

            // Value Conversion: Store enum as string in DB
            // This makes the database more readable and avoids magic numbers
            entity.Property(bl => bl.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Relationship: BookLoan belongs to one Book
            entity.HasOne(bl => bl.Book)
                .WithMany(b => b.BookLoans)
                .HasForeignKey(bl => bl.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: BookLoan belongs to one Member
            entity.HasOne(bl => bl.Member)
                .WithMany(m => m.BookLoans)
                .HasForeignKey(bl => bl.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Seed Data ----------
        // EF Core migrations will insert this data into the database
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Fiction", Description = "Literary works created from the imagination" },
            new Category { Id = 2, Name = "Non-Fiction", Description = "Writing based on facts and real events" },
            new Category { Id = 3, Name = "Science Fiction", Description = "Speculative fiction dealing with imaginative concepts" },
            new Category { Id = 4, Name = "Mystery", Description = "Fiction dealing with puzzling crimes or events" },
            new Category { Id = 5, Name = "Biography", Description = "Account of someone's life written by another person" },
            new Category { Id = 6, Name = "Technology", Description = "Books about computing, programming, and tech" },
            new Category { Id = 7, Name = "History", Description = "Books about past events and civilizations" },
            new Category { Id = 8, Name = "Self-Help", Description = "Books aimed at personal improvement" }
        );

        // Seed Authors
        modelBuilder.Entity<Author>().HasData(
            new Author { Id = 1, FirstName = "George", LastName = "Orwell", Bio = "English novelist and essayist, best known for '1984' and 'Animal Farm'.", DateOfBirth = new DateTime(1903, 6, 25) },
            new Author { Id = 2, FirstName = "Jane", LastName = "Austen", Bio = "English novelist known for her commentary on the British landed gentry.", DateOfBirth = new DateTime(1775, 12, 16) },
            new Author { Id = 3, FirstName = "Isaac", LastName = "Asimov", Bio = "American writer and professor, prolific author of science fiction.", DateOfBirth = new DateTime(1920, 1, 2) },
            new Author { Id = 4, FirstName = "Agatha", LastName = "Christie", Bio = "English writer known for detective novels featuring Hercule Poirot and Miss Marple.", DateOfBirth = new DateTime(1890, 9, 15) },
            new Author { Id = 5, FirstName = "Robert", LastName = "Martin", Bio = "American software engineer and author, known for 'Clean Code'.", DateOfBirth = new DateTime(1952, 12, 5) }
        );

        // Seed Books
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "1984", ISBN = "9780451524935", Description = "A dystopian novel set in a totalitarian society.", PublishedDate = new DateTime(1949, 6, 8), CopiesAvailable = 3, TotalCopies = 5, AuthorId = 1, CategoryId = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 2, Title = "Animal Farm", ISBN = "9780451526342", Description = "An allegorical novella reflecting events leading up to the Russian Revolution.", PublishedDate = new DateTime(1945, 8, 17), CopiesAvailable = 2, TotalCopies = 3, AuthorId = 1, CategoryId = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 3, Title = "Pride and Prejudice", ISBN = "9780141439518", Description = "A romantic novel of manners.", PublishedDate = new DateTime(1813, 1, 28), CopiesAvailable = 4, TotalCopies = 4, AuthorId = 2, CategoryId = 1, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 4, Title = "Foundation", ISBN = "9780553293357", Description = "The first novel in Asimov's Foundation series.", PublishedDate = new DateTime(1951, 5, 1), CopiesAvailable = 2, TotalCopies = 3, AuthorId = 3, CategoryId = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 5, Title = "Murder on the Orient Express", ISBN = "9780062693662", Description = "A Hercule Poirot mystery novel.", PublishedDate = new DateTime(1934, 1, 1), CopiesAvailable = 1, TotalCopies = 2, AuthorId = 4, CategoryId = 4, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Book { Id = 6, Title = "Clean Code", ISBN = "9780132350884", Description = "A handbook of agile software craftsmanship.", PublishedDate = new DateTime(2008, 8, 1), CopiesAvailable = 5, TotalCopies = 5, AuthorId = 5, CategoryId = 6, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Members
        modelBuilder.Entity<Member>().HasData(
            new Member { Id = 1, FullName = "Alice Johnson", Email = "alice@example.com", Phone = "555-0101", MembershipDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
            new Member { Id = 2, FullName = "Bob Smith", Email = "bob@example.com", Phone = "555-0102", MembershipDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc) },
            new Member { Id = 3, FullName = "Charlie Brown", Email = "charlie@example.com", Phone = "555-0103", MembershipDate = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
