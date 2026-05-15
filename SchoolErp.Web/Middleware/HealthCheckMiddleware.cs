using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace SchoolErp.Web.Middleware;

public class HealthCheckMiddleware
{
    private readonly RequestDelegate _next;

    public HealthCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, HealthCheckService healthCheckService)
    {
        if (context.Request.Path == "/health")
        {
            var report = await healthCheckService.CheckHealthAsync();

            context.Response.ContentType = "application/json";
            
            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                details = report.Entries.Select(e => new
                {
                    key = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds + "ms"
                }),
                systemInfo = new
                {
                    memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024 + "MB",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    os = Environment.OSVersion.ToString()
                },
                totalDuration = report.TotalDuration.TotalMilliseconds + "ms"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            await _next(context);
        }
    }
}
