using DBOperationWithEFCore.DTOs;

namespace DBOperationWithEFCore.Services.Interfaces;

public interface IBookService
{
    Task<PagedResult<BookDto>> GetAllAsync(int page, int pageSize, string? search, int? categoryId, int? authorId);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(CreateBookRequest request);
    Task<BookDto?> UpdateAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteAsync(int id); // Soft delete
}

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAsync();
    Task<AuthorDto?> GetByIdAsync(int id);
    Task<AuthorDto> CreateAsync(CreateAuthorRequest request);
    Task<AuthorDto?> UpdateAsync(int id, UpdateAuthorRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface IMemberService
{
    Task<PagedResult<MemberDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<MemberDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(CreateMemberRequest request);
    Task<MemberDto?> UpdateAsync(int id, UpdateMemberRequest request);
    Task<bool> DeleteAsync(int id);
}

public interface ILoanService
{
    Task<PagedResult<BookLoanDto>> GetAllAsync(int page, int pageSize, string? status);
    Task<BookLoanDto> CheckoutBookAsync(CreateLoanRequest request);
    Task<BookLoanDto> ReturnBookAsync(int loanId);
    Task<List<BookLoanDto>> GetOverdueLoansAsync();
    Task<List<BookLoanDto>> GetMemberLoansAsync(int memberId);
    Task<DashboardStats> GetDashboardStatsAsync();
}
