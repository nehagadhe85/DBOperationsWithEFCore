using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Services;

public class AuthorService : IAuthorService
{
    private readonly LibraryDbContext _context;

    public AuthorService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuthorDto>> GetAllAsync()
    {
        return await _context.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .OrderBy(a => a.LastName)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Bio = a.Bio,
                DateOfBirth = a.DateOfBirth,
                BookCount = a.Books.Count
            })
            .ToListAsync();
    }

    public async Task<AuthorDto?> GetByIdAsync(int id)
    {
        var author = await _context.Authors
            .AsNoTracking()
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null) return null;

        return new AuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            Bio = author.Bio,
            DateOfBirth = author.DateOfBirth,
            BookCount = author.Books.Count
        };
    }

    public async Task<AuthorDto> CreateAsync(CreateAuthorRequest request)
    {
        var author = new Author
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Bio = request.Bio,
            DateOfBirth = request.DateOfBirth
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(author.Id))!;
    }

    public async Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorRequest request)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null) return null;

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;
        author.Bio = request.Bio;
        author.DateOfBirth = request.DateOfBirth;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var author = await _context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (author == null) return false;

        // Prevent deletion if author has books
        if (author.Books.Any())
            throw new InvalidOperationException("Cannot delete an author who has books. Remove or reassign their books first.");

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return true;
    }
}
