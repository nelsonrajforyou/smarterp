using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;

namespace SchoolErp.Application.Features.Settings;

public class AddTableColumnCommand : IRequest<bool>
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    
    public bool IsReference { get; set; }
    public string? RefTableName { get; set; }
    public string? RefPrimaryKey { get; set; }
    public string? RefDisplayColumn { get; set; }
}

public class AddTableColumnCommandHandler : IRequestHandler<AddTableColumnCommand, bool>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AddTableColumnCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> Handle(AddTableColumnCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Security check
        if (!Regex.IsMatch(request.TableName, @"^[a-zA-Z0-9_]+$") || 
            !Regex.IsMatch(request.ColumnName, @"^[a-zA-Z0-9_]+$"))
        {
            throw new Exception("Invalid table or column name format.");
        }

        // 1. Create mapping table if it doesn't exist
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

        // 2. Add column to table
        // We ensure data type only contains safe characters (alphanumeric, spaces, parenthesis)
        if (!Regex.IsMatch(request.DataType, @"^[a-zA-Z0-9_() ]+$"))
        {
             throw new Exception("Invalid data type format.");
        }
        
        string alterTableSql = $"ALTER TABLE {request.TableName} ADD COLUMN {request.ColumnName} {request.DataType};";
        await connection.ExecuteAsync(alterTableSql);

        // 3. Save mapping if it's a reference
        if (request.IsReference && !string.IsNullOrEmpty(request.RefTableName))
        {
            string insertMappingSql = @"
                INSERT INTO DYNAMIC_COLUMN_MAPPINGS (TableName, ColumnName, RefTableName, RefPrimaryKey, RefDisplayColumn)
                VALUES (@TableName, @ColumnName, @RefTableName, @RefPrimaryKey, @RefDisplayColumn);";
                
            await connection.ExecuteAsync(insertMappingSql, new {
                TableName = request.TableName,
                ColumnName = request.ColumnName,
                RefTableName = request.RefTableName,
                RefPrimaryKey = request.RefPrimaryKey,
                RefDisplayColumn = request.RefDisplayColumn
            });
        }

        return true;
    }
}
