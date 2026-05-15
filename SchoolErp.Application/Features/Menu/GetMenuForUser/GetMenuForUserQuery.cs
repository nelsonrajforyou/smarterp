using Microsoft.Extensions.Caching.Memory;
using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;

using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Menu.GetMenuForUser;

public class GetMenuForUserQuery : IRequest<List<ModuleDto>>
{
    public string[] UserIds { get; set; } = Array.Empty<string>();
}

public class GetMenuForUserQueryHandler : IRequestHandler<GetMenuForUserQuery, List<ModuleDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMemoryCache _cache;

    public GetMenuForUserQueryHandler(IDbConnectionFactory connectionFactory, IMemoryCache cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<List<ModuleDto>> Handle(GetMenuForUserQuery request, CancellationToken cancellationToken)
    {
        if (request.UserIds == null || request.UserIds.Length == 0)
            return new List<ModuleDto>();

        string cacheKey = $"UserModuleMenu_{string.Join("_", request.UserIds.OrderBy(id => id))}";

        if (!_cache.TryGetValue(cacheKey, out List<ModuleDto>? modules))
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var dictionary = new Dictionary<string, ModuleDto>();
            
            await connection.QueryAsync<ModuleDto, MenuDto, ModuleDto>(
                MenuQueries.GetModulesWithMenusByUserId,
                (module, menu) =>
                {
                    if (!dictionary.TryGetValue(module.PARENT_ID, out var moduleEntry))
                    {
                        moduleEntry = module;
                        moduleEntry.Menus = new List<MenuDto>();
                        dictionary.Add(moduleEntry.PARENT_ID, moduleEntry);
                    }

                    if (menu != null)
                    {
                        moduleEntry.Menus.Add(menu);
                    }

                    return moduleEntry;
                },
                new { UserIds = request.UserIds },
                splitOn: "MENU_ID"
            );

            modules = dictionary.Values.OrderBy(m => m.DISPLAY_ORDER).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

            _cache.Set(cacheKey, modules, cacheOptions);
        }

        return modules ?? new List<ModuleDto>();
    }
}
