using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents a library member who can borrow books.
/// Demonstrates: Data Annotations, Unique constraint (via Fluent API), Navigation Properties.
/// </summary>
public class Member
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15)]
    public string? Phone { get; set; }

    public DateTime MembershipDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Navigation property — One Member has Many BookLoans
    public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}
