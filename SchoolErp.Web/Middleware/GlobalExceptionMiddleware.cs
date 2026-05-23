using Microsoft.AspNetCore.Http;
using SchoolErp.Application.Common.Interfaces;
using System.Security.Claims;

namespace SchoolErp.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLogger logger)
    {
        try
        {
            // Security Headers
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("Content-Security-Policy", 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://code.jquery.com https://cdn.datatables.net; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdn.jsdelivr.net https://cdn.datatables.net; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
                "img-src 'self' data:; " +
                "connect-src 'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:*;");

            await _next(context);
        }
        catch (Exception ex)
        {
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var source = context.Request.Path;
            
            await logger.LogErrorAsync(ex, userId, source);

            // Re-throw to allow standard error handling or handle here if needed
            // For Blazor, it's often better to let it bubble to the ErrorBoundary or the error page
            throw;
        }
    }
}
