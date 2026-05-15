using Dapper;
using SchoolErp.Application.Common.Mediator;
using SchoolErp.Application.Common.Interfaces;
using SchoolErp.Application.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolErp.Application.Features.Students;

public class GetDynamicStudentsQuery : IRequest<PaginatedList<IDictionary<string, object>>>
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<string> SelectedColumns { get; set; } = new();
}

public class GetDynamicStudentsQueryHandler : IRequestHandler<GetDynamicStudentsQuery, PaginatedList<IDictionary<string, object>>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetDynamicStudentsQueryHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PaginatedList<IDictionary<string, object>>> Handle(GetDynamicStudentsQuery request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (request.PageNumber - 1) * request.PageSize;

        var safeColumns = request.SelectedColumns
            .Where(c => System.Text.RegularExpressions.Regex.IsMatch(c, @"^[a-zA-Z0-9_]+$"))
            .ToList();

        if (!safeColumns.Any()) 
            safeColumns.Add("STUDENT_ID");

        if (!safeColumns.Contains("STUDENT_ID"))
            safeColumns.Insert(0, "STUDENT_ID");

        // Ensure mapping table exists
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS DYNAMIC_COLUMN_MAPPINGS (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                TableName VARCHAR(100),
                ColumnName VARCHAR(100),
                RefTableName VARCHAR(100),
                RefPrimaryKey VARCHAR(100),
                RefDisplayColumn VARCHAR(100)
            );");

        var mappings = await connection.QueryAsync<dynamic>(@"
            SELECT ColumnName, RefTableName, RefPrimaryKey, RefDisplayColumn 
            FROM DYNAMIC_COLUMN_MAPPINGS 
            WHERE TableName = 'STUDENTS_INFO'");

        var selectParts = new List<string>();
        var joinParts = new List<string>();
        int joinAliasCounter = 1;

        foreach(var col in safeColumns)
        {
            var mapping = mappings.FirstOrDefault(m => m.ColumnName == col);
            if (mapping != null && !string.IsNullOrEmpty((string)mapping.RefTableName))
            {
                string alias = $"j{joinAliasCounter++}";
                string refTable = mapping.RefTableName;
                string refPk = mapping.RefPrimaryKey;
                string refDisplay = mapping.RefDisplayColumn;

                selectParts.Add($"{alias}.{refDisplay} AS {col}_DISPLAY");
                selectParts.Add($"s.{col}"); 
                joinParts.Add($"LEFT JOIN {refTable} {alias} ON s.{col} = {alias}.{refPk}");
            }
            else if (col == "CLASS_ID" || col == "REGISTER_NO" || col == "ROLL_NO")
            {
                selectParts.Add($"sc.{col}");
            }
            else
            {
                selectParts.Add($"s.{col}");
            }
        }

        var selectClause = string.Join(", ", selectParts);

        string query = $@"
            SELECT {selectClause}
            FROM STUDENTS_INFO s
            LEFT JOIN STU_CLASS sc ON s.STUDENT_ID = sc.STUDENT_ID AND sc.IS_ACTIVE = 1 AND sc.IS_DELETED = 0
            {string.Join("\n            ", joinParts)}
            WHERE s.IS_DELETED = 0 
              AND (@SearchTerm IS NULL OR s.NAME LIKE CONCAT('%', @SearchTerm, '%'))
            ORDER BY s.CREATED_AT DESC
            LIMIT @Limit OFFSET @Offset;";

        var items = await connection.QueryAsync<dynamic>(query, 
            new { Limit = request.PageSize, Offset = offset, SearchTerm = request.SearchTerm });
            
        var totalCount = await connection.ExecuteScalarAsync<int>(
            StudentQueries.GetTotalStudentsCount,
            new { SearchTerm = request.SearchTerm });

        // Convert DapperRow to IDictionary<string, object>
        var dictList = items.Select(x => (IDictionary<string, object>)x).ToList();

        return new PaginatedList<IDictionary<string, object>>(dictList, totalCount, request.PageNumber, request.PageSize);
    }
}
