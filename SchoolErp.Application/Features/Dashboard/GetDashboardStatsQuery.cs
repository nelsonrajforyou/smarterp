using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Application.Features.Dashboard;

public class DashboardStats
{
    public string TOTAL_STUDENTS { get; set; } = "0";
    public string TOTAL_TEACHERS { get; set; } = "0";
    public string TOTAL_CLASSES { get; set; } = "0";
}

public class GetDashboardStatsQuery : IRequest<DashboardStats>
{
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStats>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDashboardStatsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardStats> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var stats = await connection.QuerySingleAsync<DashboardStats>(DashboardQueries.GetDashboardStats);
        return stats;
    }
}
