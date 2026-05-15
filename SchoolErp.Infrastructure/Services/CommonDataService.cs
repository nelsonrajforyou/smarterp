using Dapper;
using Microsoft.Extensions.Caching.Memory;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using SchoolErp.Application.Common.Queries;
using System.Data;

namespace SchoolErp.Infrastructure.Services;

public class CommonDataService : ICommonDataService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMemoryCache _cache;

    public CommonDataService(IDbConnectionFactory connectionFactory, IMemoryCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<List<DropdownItem>> GetStudentListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchStudents, cacheKey: "Common_Students");

    public async Task<List<DropdownItem>> GetStaffListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchStaff, cacheKey: "Common_Staff");

    public async Task<List<DropdownItem>> GetClassListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchClasses, cacheKey: "Common_Classes");

    public async Task<List<DropdownItem>> GetAcademicYearListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchAcademicYears, cacheKey: "Common_AcademicYears");

    public async Task<List<DropdownItem>> GetGenderListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchGenders, cacheKey: "Common_Genders");

    public async Task<List<DropdownItem>> GetStaffCategoryListAsync() 
        => await GetDropdownListAsync(CommonQueries.FetchStaffCategories, cacheKey: "Common_StaffCategories");

    public async Task<List<DropdownItem>> GetDropdownListAsync(string sql, object? parameters = null, string? cacheKey = null)
    {
        if (!string.IsNullOrEmpty(cacheKey) && _cache.TryGetValue(cacheKey, out List<DropdownItem>? cachedList))
        {
            return cachedList ?? new List<DropdownItem>();
        }

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<DropdownItem>(sql, parameters);
        var list = result.ToList();

        if (!string.IsNullOrEmpty(cacheKey))
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
            
            _cache.Set(cacheKey, list, cacheOptions);
        }

        return list;
    }
}
