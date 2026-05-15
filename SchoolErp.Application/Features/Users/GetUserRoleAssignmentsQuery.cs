using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Users;

public class GetUserRoleAssignmentsQuery : IRequest<IEnumerable<UserRoleDto>>
{
}

public class GetUserRoleAssignmentsQueryHandler : IRequestHandler<GetUserRoleAssignmentsQuery, IEnumerable<UserRoleDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetUserRoleAssignmentsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<UserRoleDto>> Handle(GetUserRoleAssignmentsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<UserRoleDto>(UserQueries.GetUserRoleAssignments);
    }
}
