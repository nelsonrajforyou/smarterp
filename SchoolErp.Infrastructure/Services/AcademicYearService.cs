using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using Dapper;

namespace SchoolErp.Infrastructure.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICommonDataService _commonData;

    public AcademicYearService(
        ProtectedSessionStorage sessionStorage, 
        IDbConnectionFactory connectionFactory,
        ICommonDataService commonData)
    {
        _sessionStorage = sessionStorage;
        _connectionFactory = connectionFactory;
        _commonData = commonData;
    }

    public event Action? OnYearChanged;

    public async Task<string> GetCurrentYearIdAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>("CurrentYearId");
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                return result.Value;
            }
        }
        catch
        {
            // Fallback for prerendering - JS interop not available
        }

        // Fallback to active year from DB
        var activeYear = await GetActiveYearFromDb();
        if (activeYear != null)
        {
            await SetCurrentYearAsync(activeYear.Value, activeYear.Text);
            return activeYear.Value;
        }

        return string.Empty;
    }

    public async Task<string> GetCurrentYearNameAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>("CurrentYearName");
            return result.Success ? result.Value ?? "Select Year" : "Select Year";
        }
        catch
        {
            return "Select Year";
        }
    }

    public async Task SetCurrentYearAsync(string yearId, string yearName)
    {
        try
        {
            await _sessionStorage.SetAsync("CurrentYearId", yearId);
            await _sessionStorage.SetAsync("CurrentYearName", yearName);
            OnYearChanged?.Invoke();
        }
        catch
        {
            // Prerendering - ignore set calls as JS interop is not available
        }
    }

    public async Task<List<DropdownItem>> GetAllYearsAsync()
    {
        return await _commonData.GetAcademicYearListAsync();
    }

    private async Task<DropdownItem?> GetActiveYearFromDb()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<DropdownItem>(
            "SELECT ACADEMIC_YEAR_ID as Value, ACADEMIC_YEAR as Text FROM ACADEMIC_YEAR WHERE IS_ACTIVE = 1 LIMIT 1");
    }
}
