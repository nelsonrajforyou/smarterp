using SchoolErp.Web.Components;
using SchoolErp.Application;
using SchoolErp.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using SchoolErp.Web.Middleware;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Infrastructure.Logging;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Error Logging
builder.Services.AddSingleton<IErrorLogger, DbErrorLogger>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "SchoolErpAuth";
        options.LoginPath = "/login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Session Expiry 7 Days
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Performance & Monitoring
builder.Services.AddHealthChecks()
    .AddCheck<SchoolErp.Infrastructure.Health.DatabaseHealthCheck>("Database")
    .AddCheck("SystemMemory", () => 
    {
        var mem = GC.GetTotalMemory(false) / 1024 / 1024;
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Current Memory Usage: {mem}MB");
    });

builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
});

// Dapper Type Handlers for DateOnly/TimeOnly
Dapper.SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
Dapper.SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

var app = builder.Build();

// Custom Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseHsts(); // Enforce HSTS even in development for Lighthouse compliance
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseOutputCache();
app.UseMiddleware<HealthCheckMiddleware>();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public class TimeOnlyTypeHandler : Dapper.SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = value.ToString("HH:mm:ss");
    }

    public override TimeOnly Parse(object value)
    {
        if (value is TimeSpan ts) return TimeOnly.FromTimeSpan(ts);
        if (value is string s && TimeOnly.TryParse(s, out var to)) return to;
        return TimeOnly.FromDateTime(Convert.ToDateTime(value));
    }
}

public class DateOnlyTypeHandler : Dapper.SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToString("yyyy-MM-dd");
    }

    public override DateOnly Parse(object value)
    {
        if (value is DateTime dt) return DateOnly.FromDateTime(dt);
        if (value is string s && DateOnly.TryParse(s, out var d)) return d;
        return DateOnly.FromDateTime(Convert.ToDateTime(value));
    }
}
