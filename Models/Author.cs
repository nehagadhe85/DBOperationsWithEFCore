using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents a book author.
/// Demonstrates: Data Annotations, One-to-Many navigation property.
/// </summary>
public class Author
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public DateTime DateOfBirth { get; set; }

    // Navigation property — One Author has Many Books
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
