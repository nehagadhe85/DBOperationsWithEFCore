using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.DTOs;

// ===== Author DTOs =====

public class AuthorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Bio { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int BookCount { get; set; }
}

public class CreateAuthorRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public DateTime DateOfBirth { get; set; }
}

public class UpdateAuthorRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public DateTime DateOfBirth { get; set; }
}
