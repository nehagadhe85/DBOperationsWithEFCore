using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents a book in the library.
/// Demonstrates: Data Annotations, Foreign Keys, Navigation Properties, Soft Delete.
/// </summary>
public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(13)]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime PublishedDate { get; set; }

    [Range(0, int.MaxValue)]
    public int CopiesAvailable { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    /// <summary>
    /// Soft delete flag — filtered by EF Core Global Query Filter.
    /// Books are never permanently deleted, only marked as inactive.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(AuthorId))]
    public Author Author { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}
