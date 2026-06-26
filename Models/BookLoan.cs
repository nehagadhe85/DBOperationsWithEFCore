using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents a book loan transaction (checkout/return).
/// Demonstrates: Composite navigation, Enum status with Value Conversion, Date tracking.
/// </summary>
public class BookLoan
{
    public int Id { get; set; }

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Loan status — stored as a string in the database via EF Core Value Conversion.
    /// </summary>
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    // Foreign Keys
    public int BookId { get; set; }
    public int MemberId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(BookId))]
    public Book Book { get; set; } = null!;

    [ForeignKey(nameof(MemberId))]
    public Member Member { get; set; } = null!;
}
