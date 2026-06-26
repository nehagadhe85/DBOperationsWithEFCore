namespace DBOperationWithEFCore.Models;

/// <summary>
/// Represents the current status of a book loan.
/// Stored as a string in the database via EF Core Value Conversion.
/// </summary>
public enum LoanStatus
{
    Active,
    Returned,
    Overdue
}
