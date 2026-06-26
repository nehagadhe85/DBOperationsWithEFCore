using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Services;

/// <summary>
/// LoanService — demonstrates advanced EF Core patterns:
/// - Explicit Transactions (BeginTransaction / Commit / Rollback)
/// - Complex LINQ queries with multiple Includes
/// - Aggregate queries (Count, Sum)
/// - Related entity updates within a transaction
/// </summary>
public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;

    public LoanService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookLoanDto>> GetAllAsync(int page, int pageSize, string? status)
    {
        var query = _context.BookLoans
            .AsNoTracking()
            .Include(bl => bl.Book)
            .Include(bl => bl.Member)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LoanStatus>(status, true, out var loanStatus))
        {
            query = query.Where(bl => bl.Status == loanStatus);
        }

        var totalCount = await query.CountAsync();

        var loans = await query
            .OrderByDescending(bl => bl.LoanDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(bl => MapToDto(bl))
            .ToListAsync();

        return new PagedResult<BookLoanDto>
        {
            Items = loans,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Checkout a book — demonstrates EF Core Transactions.
    /// Multiple operations (create loan + decrement copies) must succeed or fail together.
    /// </summary>
    public async Task<BookLoanDto> CheckoutBookAsync(CreateLoanRequest request)
    {
        // EF Core: Begin an explicit transaction
        // This ensures atomicity — both operations succeed or both fail
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validate book exists and has copies available
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null)
                throw new InvalidOperationException("Book not found.");

            if (book.CopiesAvailable <= 0)
                throw new InvalidOperationException($"No copies of '{book.Title}' are currently available.");

            // Validate member exists and is active
            var member = await _context.Members.FindAsync(request.MemberId);
            if (member == null)
                throw new InvalidOperationException("Member not found.");

            if (!member.IsActive)
                throw new InvalidOperationException($"Member '{member.FullName}' is not active.");

            // Check if member already has this book checked out
            var existingLoan = await _context.BookLoans
                .AnyAsync(bl => bl.BookId == request.BookId
                    && bl.MemberId == request.MemberId
                    && bl.Status == LoanStatus.Active);

            if (existingLoan)
                throw new InvalidOperationException("This member already has an active loan for this book.");

            // Create the loan
            var loan = new BookLoan
            {
                BookId = request.BookId,
                MemberId = request.MemberId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(request.LoanDurationDays),
                Status = LoanStatus.Active
            };

            _context.BookLoans.Add(loan);

            // Decrement available copies
            book.CopiesAvailable--;

            await _context.SaveChangesAsync();

            // EF Core: Commit the transaction — both changes are now permanent
            await transaction.CommitAsync();

            // Reload with navigation properties for the response
            var createdLoan = await _context.BookLoans
                .AsNoTracking()
                .Include(bl => bl.Book)
                .Include(bl => bl.Member)
                .FirstAsync(bl => bl.Id == loan.Id);

            return MapToDto(createdLoan);
        }
        catch
        {
            // EF Core: Rollback on any error — neither change is persisted
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Return a book — also uses a transaction to ensure atomicity.
    /// </summary>
    public async Task<BookLoanDto> ReturnBookAsync(int loanId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var loan = await _context.BookLoans
                .Include(bl => bl.Book)
                .Include(bl => bl.Member)
                .FirstOrDefaultAsync(bl => bl.Id == loanId);

            if (loan == null)
                throw new InvalidOperationException("Loan not found.");

            if (loan.Status == LoanStatus.Returned)
                throw new InvalidOperationException("This book has already been returned.");

            // Mark as returned
            loan.Status = LoanStatus.Returned;
            loan.ReturnDate = DateTime.UtcNow;

            // Increment available copies
            loan.Book.CopiesAvailable++;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(loan);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<BookLoanDto>> GetOverdueLoansAsync()
    {
        // Complex LINQ query: find loans that are active and past their due date
        return await _context.BookLoans
            .AsNoTracking()
            .Include(bl => bl.Book)
            .Include(bl => bl.Member)
            .Where(bl => bl.Status == LoanStatus.Active && bl.DueDate < DateTime.UtcNow)
            .OrderBy(bl => bl.DueDate)
            .Select(bl => MapToDto(bl))
            .ToListAsync();
    }

    public async Task<List<BookLoanDto>> GetMemberLoansAsync(int memberId)
    {
        return await _context.BookLoans
            .AsNoTracking()
            .Include(bl => bl.Book)
            .Include(bl => bl.Member)
            .Where(bl => bl.MemberId == memberId)
            .OrderByDescending(bl => bl.LoanDate)
            .Select(bl => MapToDto(bl))
            .ToListAsync();
    }

    /// <summary>
    /// Dashboard statistics — demonstrates aggregate LINQ queries.
    /// </summary>
    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var stats = new DashboardStats
        {
            TotalBooks = await _context.Books.CountAsync(),
            TotalMembers = await _context.Members.CountAsync(),
            TotalAuthors = await _context.Authors.CountAsync(),
            TotalCategories = await _context.Categories.CountAsync(),
            ActiveLoans = await _context.BookLoans
                .CountAsync(bl => bl.Status == LoanStatus.Active),
            OverdueLoans = await _context.BookLoans
                .CountAsync(bl => bl.Status == LoanStatus.Active && bl.DueDate < DateTime.UtcNow),
            RecentLoans = await _context.BookLoans
                .AsNoTracking()
                .Include(bl => bl.Book)
                .Include(bl => bl.Member)
                .OrderByDescending(bl => bl.LoanDate)
                .Take(5)
                .Select(bl => MapToDto(bl))
                .ToListAsync()
        };

        return stats;
    }

    // ---- Private Helpers ----

    private static BookLoanDto MapToDto(BookLoan loan) => new()
    {
        Id = loan.Id,
        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        Status = loan.Status.ToString(),
        BookId = loan.BookId,
        BookTitle = loan.Book.Title,
        BookISBN = loan.Book.ISBN,
        MemberId = loan.MemberId,
        MemberName = loan.Member.FullName
    };
}
