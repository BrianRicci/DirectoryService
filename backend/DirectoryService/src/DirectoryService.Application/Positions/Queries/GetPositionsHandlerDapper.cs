using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using Shared.SharedKernel;

namespace DirectoryService.Application.Positions.Queries;

public sealed class GetPositionsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetPositionsHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<PaginationResponse<GetPositionDto>?, Errors>> Handle(
        GetPositionsRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("p.name ILIKE @search");
            parameters.Add("search", $"%{query.Search}%");
        }

        if (query.IsActive.HasValue)
        {
            conditions.Add("p.is_active = @is_active");
            parameters.Add("is_active", query.IsActive);
        }

        if (query.DepartmentIds?.Count > 0)
        {
            conditions.Add("d.department_id = ANY(@department_ids)");
            parameters.Add("department_ids", query.DepartmentIds);
        }

        parameters.Add("offset", (query.Page - 1) * query.PageSize, DbType.Int32);
        parameters.Add("page_size", query.PageSize, DbType.Int32);

        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        string sortOrder = query.sortOrder?.ToLowerInvariant() == "desc" ? "DESC" : "ASC";

        string sortBy = query.sortBy?.ToLowerInvariant() switch
        {
            "createdAt" => "p.created_at",
            _ => "p.name",
        };

        string orderByString = $"ORDER BY {sortBy} {sortOrder}";

        long? totalCount = 0;

        long positionsCount = 0;

        var positionsDict = new Dictionary<Guid, GetPositionDto>();
        
        await connection
            .QueryAsync<GetPositionDto, DepartmentDto, long, long, GetPositionDto>(
                $"""
                 WITH positions_stats AS (
                     SELECT COUNT(*) as count
                     FROM positions
                 )

                 SELECT p.position_id,
                        p.name,
                        p.description,
                        p.is_active,
                        p.created_at,
                        p.updated_at,
                        d.department_id,
                        d.name,
                        d.identifier,
                        d.parent_id,
                        d.path,
                        d.depth,
                        d.is_active,
                        d.created_at,
                        d.updated_at,
                        COUNT(*) OVER () AS total_count,
                        ls.count as positions_count 
                 FROM positions p
                     CROSS JOIN positions_stats ls
                     LEFT JOIN department_positions dp ON dp.position_id = p.position_id
                     LEFT JOIN departments d ON d.department_id = dp.department_id AND d.is_active = true
                 {whereClause}
                 {orderByString}
                 LIMIT @page_size OFFSET @offset
                 """,
                param: parameters,
                splitOn: "department_id, total_count, positions_count",
                map: (position, department, inRequestCount, allPositionsCount) =>
                {
                    // position mapping
                    if (!positionsDict.TryGetValue(position.PositionId, out var existingPosition))
                    {
                        existingPosition = position;
                        
                        positionsDict.Add(existingPosition.PositionId, existingPosition);
                    }

                    if (department is not null && department.DepartmentId != Guid.Empty)
                    {
                        if (existingPosition.Departments.All(d => d.DepartmentId != department.DepartmentId))
                            existingPosition.Departments.Add(department);
                    }

                    // total_count mapping
                    totalCount = inRequestCount;

                    positionsCount = allPositionsCount;

                    return existingPosition;
                });

        var positions = positionsDict.Values.ToList();
        
        long totalPages = (long)Math.Ceiling(positionsCount / (double)query.PageSize);

        return new PaginationResponse<GetPositionDto>(
            positions.ToList(), totalCount ?? 0, query.Page, query.PageSize, totalPages);
    }
}