using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Settings;

public class ColumnDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? RefTableName { get; set; }
    public string? RefPrimaryKey { get; set; }
    public string? RefDisplayColumn { get; set; }
}

public class GetTableColumnsQuery : IRequest<List<ColumnDto>>
{
    public string TableName { get; set; } = string.Empty;
}

public class GetTableColumnsQueryHandler : IRequestHandler<GetTableColumnsQuery, List<ColumnDto>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetTableColumnsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ColumnDto>> Handle(GetTableColumnsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Ensure mapping table exists before querying it
        string createMappingTableSql = @"
            CREATE TABLE IF NOT EXISTS DYNAMIC_COLUMN_MAPPINGS (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TableName VARCHAR(100),
                ColumnName VARCHAR(100),
                RefTableName VARCHAR(100),
                RefPrimaryKey VARCHAR(100),
                RefDisplayColumn VARCHAR(100)
            );";
        await connection.ExecuteAsync(createMappingTableSql);

        string sql = @"
            SELECT 
                c.COLUMN_NAME as ColumnName, 
                c.DATA_TYPE as DataType,
                m.RefTableName,
                m.RefPrimaryKey,
                m.RefDisplayColumn
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN DYNAMIC_COLUMN_MAPPINGS m ON c.TABLE_NAME = m.TableName AND c.COLUMN_NAME = m.ColumnName
            WHERE c.TABLE_SCHEMA = DATABASE() AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION;";

        var result = await connection.QueryAsync<ColumnDto>(sql, new { TableName = request.TableName });
        return result.AsList();
    }
}
