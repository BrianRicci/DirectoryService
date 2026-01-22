using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using Shared.SharedKernel;

namespace DirectoryService.Application.Locations.Queries;

public class GetLocationsHandlerDapper
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetLocationsHandlerDapper(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<PaginationResponse<GetLocationDto>?, Errors>> Handle(GetLocationsRequest query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        
        var parameters = new DynamicParameters();

        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add("l.name ILIKE @search");
            parameters.Add("search", $"%{query.Search}%");
        }

        if (query.IsActive.HasValue)
        {
            conditions.Add("l.is_active = @is_active");
            parameters.Add("is_active", query.IsActive);
        }
        
        if (query.DepartmentIds?.Count > 0)
        {
            conditions.Add("d.department_id = ANY(@department_ids)");
            parameters.Add("department_ids", query.DepartmentIds);
        }

        var pagination = query.Pagination;

        parameters.Add("offset", (pagination.Page - 1) * pagination.PageSize, DbType.Int32);
        parameters.Add("page_size", pagination.PageSize, DbType.Int32);
        
        string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;

        long? totalCount = 0;

        long locationsCount = 0;
        
        var locations = await connection.QueryAsync<GetLocationDto, LocationAddressDto, DepartmentDto, long, long, GetLocationDto>(
            $"""
            WITH locations_stats AS (
                SELECT COUNT(*) as count
                FROM locations
            )

            SELECT l.location_id,
                   l.name, 
                   l.is_active,
                   l.timezone,
                   l.created_at,
                   l.updated_at,
                   l.country,
                   l.region,
                   l.city,
                   l.street,
                   l.house,
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
                   ls.count as locations_count 
            FROM locations l
                CROSS JOIN locations_stats ls
                LEFT JOIN department_locations dl ON dl.location_id = l.location_id
                LEFT JOIN departments d ON d.department_id = dl.department_id
            {whereClause}
            ORDER BY l.name
            LIMIT @page_size OFFSET @offset
            """,
            param: parameters,
            splitOn: "country, department_id, total_count, locations_count",
            map: (location, address, department, inRequestCount, allLocationsCount) =>
            {
                // address mapping
                location.Address = address;
                
                // department mapping
                location.Departments.Add(department);

                // total_count mapping
                totalCount = inRequestCount;

                locationsCount = allLocationsCount;

                return location;
            });

        long totalPages = (long)Math.Ceiling(locationsCount / (double)pagination.PageSize);
        
        return new PaginationResponse<GetLocationDto>(
            locations.ToList(), totalCount ?? 0, pagination.Page, pagination.PageSize, totalPages);
    }
}