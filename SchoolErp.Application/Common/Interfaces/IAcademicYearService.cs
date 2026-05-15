using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Common.Interfaces;

public interface IAcademicYearService
{
    Task<string> GetCurrentYearIdAsync();
    Task<string> GetCurrentYearNameAsync();
    Task SetCurrentYearAsync(string yearId, string yearName);
    Task<List<DropdownItem>> GetAllYearsAsync();
    event Action? OnYearChanged;
}
