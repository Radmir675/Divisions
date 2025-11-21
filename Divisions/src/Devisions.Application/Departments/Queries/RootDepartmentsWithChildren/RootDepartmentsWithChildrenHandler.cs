using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Contracts.Departments.Responses;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.RootDepartmentsWithChildren;

public class RootDepartmentsWithChildrenHandler : IQueryHandler<IReadOnlyList<DepartmentBaseDto>,
    RootDepartmentsWithChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<RootDepartmentsWithChildrenHandler> _logger;

    public RootDepartmentsWithChildrenHandler(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<RootDepartmentsWithChildrenHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DepartmentBaseDto>, Errors>> Handle(
        RootDepartmentsWithChildrenQuery withChildrenQuery,
        CancellationToken cancellationToken)
    {
        const string sql = $"""
                            WITH root AS (SELECT id,
                                                 identifier,
                                                 parent_id,
                                                 path,
                                                 depth,
                                                 is_active,
                                                 created_at,
                                                 updated_at,
                                                 name
                                          FROM departments
                                          WHERE parent_id is null
                                          ORDER BY created_at DESC
                                          OFFSET @offset 
                                          LIMIT @root_limit)


                            SELECT *,
                                   EXISTS (SELECT 1
                                           FROM departments d
                                           WHERE d.parent_id = root.id
                                           OFFSET @child_limit LIMIT 1) as HasMoreChildren
                            FROM root

                            UNION

                            SELECT cd.*,
                                   EXISTS (SELECT 1 FROM departments d WHERE parent_id = cd.id) as hasMoreChildren
                            FROM root r
                                     CROSS JOIN LATERAL (
                                SELECT *
                                FROM departments d
                                WHERE d.parent_id = r.id
                                  AND d.is_active = true
                                ORDER BY d.created_at
                                LIMIT @child_limit
                                ) AS cd

                            """;

        int prefetch = withChildrenQuery.Prefetch!.Value;
        int page = withChildrenQuery.Request.Page!.Value;
        int size = withChildrenQuery.Request.Size!.Value;

        var sqlParams = new { offset = (page - 1) * size, root_limit = size, child_limit = prefetch, };
        using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);

        var result = await connection
            .QueryAsync<DepartmentBaseDto>(sql, sqlParams);

        var resultCollection = result.ToList();

        _logger.LogInformation("Data received");

        return resultCollection;
    }
}