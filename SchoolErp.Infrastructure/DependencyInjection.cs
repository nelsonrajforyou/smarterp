using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Domain.Entities;
using SchoolErp.Application.Common.Security;
using SchoolErp.Infrastructure.Auth;
using SchoolErp.Infrastructure.Data;

using SchoolErp.Infrastructure.Services;

namespace SchoolErp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher<User>, SecurePasswordHasher>();
        services.AddMemoryCache();
        
        services.AddScoped<ICommonDataService, CommonDataService>();
        services.AddScoped<IAcademicService, AcademicService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        return services;
    }
}
