using Dapper;
using SchoolErp.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.IO;

namespace SchoolErp.Infrastructure.Logging;

public class DbErrorLogger : IErrorLogger
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;

    public DbErrorLogger(IDbConnectionFactory connectionFactory, IConfiguration configuration)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
    }

    public async Task LogErrorAsync(Exception ex, string? userId = null, string? source = null)
    {
        await LogToDb("Error", ex.Message, ex.StackTrace, source, userId);
    }

    public async Task LogWarningAsync(string message, string? userId = null, string? source = null)
    {
        await LogToDb("Warning", message, null, source, userId);
    }

    public async Task LogInfoAsync(string message, string? userId = null, string? source = null)
    {
        await LogToDb("Info", message, null, source, userId);
    }

    private async Task LogToDb(string level, string message, string? stackTrace, string? source, string? userId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO SYSTEM_LOGS (LOG_LEVEL, MESSAGE, STACK_TRACE, SOURCE, USER_ID)
                VALUES (@Level, @Message, @StackTrace, @Source, @UserId);";

            await connection.ExecuteAsync(sql, new
            {
                Level = level,
                Message = message,
                StackTrace = stackTrace,
                Source = source,
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            // Fail-safe: Fallback to File, Console and Trace if DB is down
            var fallbackMessage = $"[{timestamp}] FALLBACK_{level}: {message} | Source: {source} | User: {userId} | Stack: {stackTrace} | Fallback Error: {ex.Message}";
            
            System.Diagnostics.Trace.WriteLine(fallbackMessage);
            Console.WriteLine(fallbackMessage);

            try
            {
                var logPath = _configuration["Logging:FallbackLogPath"] ?? "logs/fallback.log";
                var directory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.AppendAllTextAsync(logPath, fallbackMessage + Environment.NewLine);
            }
            catch
            {
                // Last resort: if file writing also fails, we've already logged to Console/Trace
            }
        }
    }
}
