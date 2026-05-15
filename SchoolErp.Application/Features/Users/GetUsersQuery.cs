using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Users;

public class UserDto
{
    public string USER_INFO_ID { get; set; } = default!;
    public string EMAIL { get; set; } = default!;
    public string FIRST_NAME { get; set; } = default!;
    public string LAST_NAME { get; set; } = default!;
    public string ROLE_ID { get; set; } = default!;
    public string ROLE_NAME { get; set; } = default!;
    public string USER_ID { get; set; } = default!; // Staff/Student ID
    public string STATUS { get; set; } = default!;
    public string IS_ACTIVE { get; set; } = default!;
    public string IS_DELETED { get; set; } = default!;
    public string CREATED_AT { get; set; } = default!;
}

public class GetUsersQuery : IRequest<PaginatedList<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetUsersQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var offset = (request.PageNumber - 1) * request.PageSize;
        
        var items = await connection.QueryAsync<UserDto>(
            UserQueries.GetAllUsers, 
            new { Limit = request.PageSize, Offset = offset });
            
        var totalCount = await connection.ExecuteScalarAsync<int>(UserQueries.GetTotalUsersCount);

        return new PaginatedList<UserDto>(items.ToList(), totalCount, request.PageNumber, request.PageSize);
    }
}
