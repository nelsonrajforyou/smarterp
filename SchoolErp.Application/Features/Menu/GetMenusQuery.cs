using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;

namespace SchoolErp.Application.Features.Menu;

public class GetMenusQuery : IRequest<IEnumerable<MenuDto>>
{
}

public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, IEnumerable<MenuDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetMenusQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<MenuDto>> Handle(GetMenusQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<MenuDto>(MenuQueries.GetAllMenus);
    }
}
