using Dapper;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Students;

public class StudentStatsDto
{
    public int TotalStudents { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int NewAdmissionsCount { get; set; }
}

public class GetStudentStatsQuery : IRequest<StudentStatsDto>
{
}

public class GetStudentStatsQueryHandler : IRequestHandler<GetStudentStatsQuery, StudentStatsDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetStudentStatsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<StudentStatsDto> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT 
                COUNT(*) as TotalStudents,
                SUM(CASE WHEN GENDER_ID = 'Male' THEN 1 ELSE 0 END) as MaleCount,
                SUM(CASE WHEN GENDER_ID = 'Female' THEN 1 ELSE 0 END) as FemaleCount,
                SUM(CASE WHEN CREATED_AT >= DATE_SUB(NOW(), INTERVAL 30 DAY) THEN 1 ELSE 0 END) as NewAdmissionsCount
            FROM STUDENTS_INFO 
            WHERE IS_DELETED = 0;";

        return await connection.QuerySingleAsync<StudentStatsDto>(sql);
    }
}
