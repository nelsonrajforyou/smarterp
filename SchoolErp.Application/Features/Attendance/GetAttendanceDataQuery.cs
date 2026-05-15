using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Attendance;

public class GetAttendanceDataQuery : IRequest<AttendanceDataResponse>
{
    public string ClassId { get; set; } = default!;
    public string AttendanceDate { get; set; } = default!;
}

public class AttendanceDataResponse
{
    public List<StudentAttendanceDto> Students { get; set; } = new();
    public List<AttendanceTypeDto> AttendanceTypes { get; set; } = new();
}

public class GetAttendanceDataQueryHandler : IRequestHandler<GetAttendanceDataQuery, AttendanceDataResponse>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAttendanceDataQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AttendanceDataResponse> Handle(GetAttendanceDataQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var students = await connection.QueryAsync<StudentAttendanceDto>(
            AttendanceQueries.GetStudentsForAttendance, 
            new { ClassId = request.ClassId, AttendanceDate = request.AttendanceDate });
            
        var types = await connection.QueryAsync<AttendanceTypeDto>(
            AttendanceQueries.GetAttendanceTypes);

        return new AttendanceDataResponse
        {
            Students = students.ToList(),
            AttendanceTypes = types.ToList()
        };
    }
}
