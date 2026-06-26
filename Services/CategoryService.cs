using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Services;

public class CategoryService : ICategoryService
{
    private readonly LibraryDbContext _context;

    public CategoryService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Books)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                BookCount = c.Books.Count
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            BookCount = category.Books.Count
        };
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(category.Id))!;
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = request.Name;
        category.Description = request.Description;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Books)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        if (category.Books.Any())
            throw new InvalidOperationException("Cannot delete a category that has books. Remove or reassign books first.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }
}
