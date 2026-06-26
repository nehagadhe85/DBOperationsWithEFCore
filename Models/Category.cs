using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents a book category/genre.
/// Demonstrates: Data Annotations, One-to-Many navigation property.
/// </summary>
public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation property — One Category has Many Books
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
