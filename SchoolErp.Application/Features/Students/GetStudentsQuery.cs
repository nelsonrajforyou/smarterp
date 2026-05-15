using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Students;

public class StudentDto
{
    public string STUDENT_ID { get; set; } = default!;
    public string NAME { get; set; } = default!;
    public string MOBILE_NO { get; set; } = default!;
    public string FATHER_NAME { get; set; } = default!;
    public string MOTHER_NAME { get; set; } = default!;
    public string ADDRESS { get; set; } = default!;
    public string ADMISSION_NO { get; set; } = default!;
    public string EMAIL_ID { get; set; } = default!;
    public string GENDER_ID { get; set; } = default!;
    public string DATE_OF_BIRTH { get; set; } = default!;
    public string? STATE_ID { get; set; }
    public string? USER_ID { get; set; }
    public string? NATIONALITY_ID { get; set; }
    public string CLASS_ID { get; set; } = default!; // From STU_CLASS join
    public string REGISTER_NO { get; set; } = default!;
    public string ROLL_NO { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = default!;
    public string IS_DELETED { get; set; } = default!;
    public string CREATED_AT { get; set; } = default!;
}

public class GetStudentsQuery : IRequest<PaginatedList<StudentDto>>
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, PaginatedList<StudentDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetStudentsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PaginatedList<StudentDto>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var offset = (request.PageNumber - 1) * request.PageSize;
        
        var items = await connection.QueryAsync<StudentDto>(
            StudentQueries.GetAllStudents, 
            new { Limit = request.PageSize, Offset = offset, SearchTerm = request.SearchTerm });
            
        var totalCount = await connection.ExecuteScalarAsync<int>(
            StudentQueries.GetTotalStudentsCount,
            new { SearchTerm = request.SearchTerm });

        return new PaginatedList<StudentDto>(items.ToList(), totalCount, request.PageNumber, request.PageSize);
    }
}
