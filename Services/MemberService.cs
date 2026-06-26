using Microsoft.EntityFrameworkCore;
using DBOperationWithEFCore.Data;
using DBOperationWithEFCore.DTOs;
using DBOperationWithEFCore.Models;
using DBOperationWithEFCore.Services.Interfaces;

namespace DBOperationWithEFCore.Services;

public class MemberService : IMemberService
{
    private readonly LibraryDbContext _context;

    public MemberService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MemberDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _context.Members
            .AsNoTracking()
            .Include(m => m.BookLoans)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(m =>
                m.FullName.ToLower().Contains(search) ||
                m.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var members = await query
            .OrderBy(m => m.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberDto
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Phone = m.Phone,
                MembershipDate = m.MembershipDate,
                IsActive = m.IsActive,
                ActiveLoansCount = m.BookLoans.Count(bl => bl.Status == LoanStatus.Active)
            })
            .ToListAsync();

        return new PagedResult<MemberDto>
        {
            Items = members,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MemberDto?> GetByIdAsync(int id)
    {
        var member = await _context.Members
            .AsNoTracking()
            .Include(m => m.BookLoans)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return null;

        return new MemberDto
        {
            Id = member.Id,
            FullName = member.FullName,
            Email = member.Email,
            Phone = member.Phone,
            MembershipDate = member.MembershipDate,
            IsActive = member.IsActive,
            ActiveLoansCount = member.BookLoans.Count(bl => bl.Status == LoanStatus.Active)
        };
    }

    public async Task<MemberDto> CreateAsync(CreateMemberRequest request)
    {
        // Check for duplicate email
        var existingMember = await _context.Members
            .AnyAsync(m => m.Email.ToLower() == request.Email.ToLower());

        if (existingMember)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        var member = new Member
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(member.Id))!;
    }

    public async Task<MemberDto?> UpdateAsync(int id, UpdateMemberRequest request)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null) return null;

        // Check for duplicate email (excluding current member)
        var emailExists = await _context.Members
            .AnyAsync(m => m.Email.ToLower() == request.Email.ToLower() && m.Id != id);

        if (emailExists)
            throw new InvalidOperationException($"A member with email '{request.Email}' already exists.");

        member.FullName = request.FullName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _context.Members
            .Include(m => m.BookLoans)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member == null) return false;

        if (member.BookLoans.Any(bl => bl.Status == LoanStatus.Active))
            throw new InvalidOperationException("Cannot delete a member who has active book loans. Return all books first.");

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return true;
    }
}
