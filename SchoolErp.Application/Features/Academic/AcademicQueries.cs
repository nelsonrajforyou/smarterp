using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Queries;
using Dapper;

namespace SchoolErp.Application.Features.Academic;

public record GetClassesWithStaffQuery : IRequest<List<ClassDto>>;
public record GetAcademicYearsQuery : IRequest<List<AcademicYearDto>>;

public class ClassDto
{
    public string ClassId { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string Section { get; set; } = "";
    public string TeacherId { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public string IsActive { get; set; } = "1";
}

public class AcademicYearDto
{
    public string YearId { get; set; } = "";
    public string YearName { get; set; } = "";
    public string FromDate { get; set; } = "";
    public string ToDate { get; set; } = "";
    public string IsActive { get; set; } = "0";
}

public class GetClassesWithStaffQueryHandler : IRequestHandler<GetClassesWithStaffQuery, List<ClassDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetClassesWithStaffQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ClassDto>> Handle(GetClassesWithStaffQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<dynamic>(AcademicQueries.GetAllClassesWithStaff);
        
        return result.Select(r => new ClassDto
        {
            ClassId = r.CLASS_ID.ToString(),
            ClassName = r.CLASS_NAME,
            Section = r.SECTION,
            TeacherId = r.TEACHER_ID?.ToString() ?? "",
            TeacherName = r.TeacherName ?? "",
            IsActive = r.IS_ACTIVE.ToString()
        }).ToList();
    }
}

public class GetAcademicYearsQueryHandler : IRequestHandler<GetAcademicYearsQuery, List<AcademicYearDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAcademicYearsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<dynamic>(AcademicQueries.GetAllAcademicYears);
        
        return result.Select(r => new AcademicYearDto
        {
            YearId = r.ACADEMIC_YEAR_ID.ToString(),
            YearName = r.ACADEMIC_YEAR,
            FromDate = r.FROM_DATE,
            ToDate = r.TO_DATE,
            IsActive = r.IS_ACTIVE.ToString()
        }).ToList();
    }
}
