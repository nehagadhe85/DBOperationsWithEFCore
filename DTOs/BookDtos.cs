using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.DTOs;

// ===== Book DTOs =====

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PublishedDate { get; set; }
    public int CopiesAvailable { get; set; }
    public int TotalCopies { get; set; }
    public DateTime CreatedAt { get; set; }

    // Flattened navigation data
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateBookRequest
{
    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(13)]
    public string ISBN { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime PublishedDate { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

public class UpdateBookRequest
{
    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int CopiesAvailable { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalCopies { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required]
    public int CategoryId { get; set; }
}
