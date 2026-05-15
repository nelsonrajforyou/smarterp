using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;

namespace SchoolErp.Application.Features.Users;

public class AcademicYearDto
{
    public string ACADEMIC_YEAR_ID { get; set; } = string.Empty;
    public string ACADEMIC_YEAR { get; set; } = string.Empty;
}

public class GetAcademicYearsQuery : IRequest<IEnumerable<AcademicYearDto>> { }

public class GetAcademicYearsQueryHandler : IRequestHandler<GetAcademicYearsQuery, IEnumerable<AcademicYearDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetAcademicYearsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<AcademicYearDto>("SELECT ACADEMIC_YEAR_ID, ACADEMIC_YEAR FROM ACADEMIC_YEAR WHERE IS_DELETED = 0;");
    }
}
