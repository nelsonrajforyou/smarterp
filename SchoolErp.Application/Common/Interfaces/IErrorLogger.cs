namespace SchoolErp.Application.Common.Interfaces;

public interface IErrorLogger
{
    Task LogErrorAsync(Exception ex, string? userId = null, string? source = null);
    Task LogWarningAsync(string message, string? userId = null, string? source = null);
    Task LogInfoAsync(string message, string? userId = null, string? source = null);
}
