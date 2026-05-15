using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Menu;

public class GetRoleMenuAssignmentsQuery : IRequest<IEnumerable<RoleMenuDto>>
{
}

public class GetRoleMenuAssignmentsQueryHandler : IRequestHandler<GetRoleMenuAssignmentsQuery, IEnumerable<RoleMenuDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetRoleMenuAssignmentsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<RoleMenuDto>> Handle(GetRoleMenuAssignmentsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<RoleMenuDto>(MenuQueries.GetRoleMenuAssignments);
    }
}
