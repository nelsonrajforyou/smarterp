using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Common.Interfaces;

public interface ICommonDataService
{
    Task<List<DropdownItem>> GetStudentListAsync();
    Task<List<DropdownItem>> GetStaffListAsync();
    Task<List<DropdownItem>> GetClassListAsync();
    Task<List<DropdownItem>> GetAcademicYearListAsync();
    Task<List<DropdownItem>> GetGenderListAsync();
    Task<List<DropdownItem>> GetStaffCategoryListAsync();
    
    /// <summary>
    /// Generic method to fetch any dropdown list using a custom query
    /// </summary>
    Task<List<DropdownItem>> GetDropdownListAsync(string sql, object? parameters = null, string? cacheKey = null);
}
