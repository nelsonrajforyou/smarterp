using Microsoft.Extensions.Diagnostics.HealthChecks;
using SchoolErp.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Infrastructure.Health;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var conn = _connectionFactory.CreateConnection();
            if (conn is System.Data.Common.DbConnection dbConn)
                await dbConn.OpenAsync(cancellationToken);
            else
                conn.Open();
            return HealthCheckResult.Healthy("Database is responding.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database is down: {ex.Message}");
        }
    }
}
