using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Services;

/// <summary>
/// Book service demonstrating key EF Core query patterns:
/// - Eager Loading with .Include()
/// - AsNoTracking for read-only performance
/// - LINQ queries for filtering and searching
/// - Skip/Take for pagination
/// - Soft Delete pattern
/// </summary>
public class BookService : IBookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BookDto>> GetAllAsync(
        int page, int pageSize, string? search, int? categoryId, int? authorId)
    {
        // Start with a query — note: query filters automatically exclude soft-deleted books
        var query = _context.Books
            .AsNoTracking()                    // EF Core: No change tracking for read-only queries (better performance)
            .Include(b => b.Author)            // EF Core: Eager Loading — loads Author data in same query
            .Include(b => b.Category)          // EF Core: Eager Loading — loads Category data in same query
            .AsQueryable();

        // Apply search filter (LINQ Where clause)
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(search) ||
                b.ISBN.Contains(search) ||
                b.Author.FirstName.ToLower().Contains(search) ||
                b.Author.LastName.ToLower().Contains(search));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            query = query.Where(b => b.CategoryId == categoryId.Value);
        }

        // Apply author filter
        if (authorId.HasValue)
        {
            query = query.Where(b => b.AuthorId == authorId.Value);
        }

        // Get total count BEFORE pagination (for PagedResult metadata)
        var totalCount = await query.CountAsync();

        // Apply pagination — Skip/Take pattern
        var books = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)       // EF Core: Skip N records
            .Take(pageSize)                     // EF Core: Take N records
            .Select(b => MapToDto(b))           // Project to DTO
            .ToListAsync();

        return new PagedResult<BookDto>
        {
            Items = books,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _context.Books
            .AsNoTracking()
            .Include(b => b.Author)
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        return book == null ? null : MapToDto(book);
    }

    public async Task<BookDto> CreateAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            ISBN = request.ISBN,
            Description = request.Description,
            PublishedDate = request.PublishedDate,
            TotalCopies = request.TotalCopies,
            CopiesAvailable = request.TotalCopies, // New books: all copies are available
            AuthorId = request.AuthorId,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Books.Add(book);           // EF Core: Mark entity for insertion
        await _context.SaveChangesAsync();  // EF Core: Execute the INSERT SQL

        // Reload with navigation properties
        return (await GetByIdAsync(book.Id))!;
    }

    public async Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request)
    {
        // EF Core: Find the tracked entity (WITH change tracking for updates)
        var book = await _context.Books.FindAsync(id);
        if (book == null) return null;

        // Update properties — EF Core tracks these changes automatically
        book.Title = request.Title;
        book.Description = request.Description;
        book.CopiesAvailable = request.CopiesAvailable;
        book.TotalCopies = request.TotalCopies;
        book.AuthorId = request.AuthorId;
        book.CategoryId = request.CategoryId;

        await _context.SaveChangesAsync();  // EF Core: Execute the UPDATE SQL (only changed columns)

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return false;

        // Soft Delete — set the flag instead of actually removing
        // The Global Query Filter will hide this book from all future queries
        book.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }

    // ---- Private Helpers ----

    private static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        ISBN = book.ISBN,
        Description = book.Description,
        PublishedDate = book.PublishedDate,
        CopiesAvailable = book.CopiesAvailable,
        TotalCopies = book.TotalCopies,
        CreatedAt = book.CreatedAt,
        AuthorId = book.AuthorId,
        AuthorName = $"{book.Author.FirstName} {book.Author.LastName}",
        CategoryId = book.CategoryId,
        CategoryName = book.Category.Name
    };
}
