using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Students;

public class GetStudentByIdQuery : IRequest<StudentDto>
{
    public string Id { get; set; } = string.Empty;
}

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetStudentByIdQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<StudentDto>(StudentQueries.GetStudentById, new { Id = request.Id });
    }
}
