using System.ComponentModel.DataAnnotations;

namespace DBOperationWithEFCore.DTOs;

// ===== BookLoan DTOs =====

public class BookLoanDto
{
    public int Id { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;

    // Flattened navigation data
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookISBN { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
}

public class CreateLoanRequest
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }

    /// <summary>
    /// Number of days for the loan. Default is 14 days.
    /// </summary>
    [Range(1, 90)]
    public int LoanDurationDays { get; set; } = 14;
}

public class ReturnLoanRequest
{
    [Required]
    public int LoanId { get; set; }
}

// ===== Dashboard DTOs =====

public class DashboardStats
{
    public int TotalBooks { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalAuthors { get; set; }
    public int TotalCategories { get; set; }
    public List<BookLoanDto> RecentLoans { get; set; } = new();
}
