using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Application.Departments.Queries.RootDepartmentsWithChildren;
using Devisions.Application.Extensions;
using Devisions.Contracts.Departments.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.DepartmentChildren;

public class
    DepartmentChildrenHandler : IQueryHandler<IReadOnlyList<DepartmentBaseDto>, DepartmentChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<DepartmentChildrenQuery> _validator;
    private readonly ILogger<RootDepartmentsWithChildrenHandler> _logger;

    public DepartmentChildrenHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<DepartmentChildrenQuery> validator,
        ILogger<RootDepartmentsWithChildrenHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DepartmentBaseDto>, Errors>> Handle(
        DepartmentChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        const string sqlQuery =
            """
            WITH dep AS (SELECT id,
                                       identifier,
                                       parent_id,
                                       path,
                                       depth,
                                       is_active,
                                       created_at,
                                       updated_at,
                                       name
                                FROM departments
                                WHERE parent_id = @parent_id
                                ORDER BY created_at DESC
                                OFFSET @offset 
                                    LIMIT @limit)

            SELECT dep.*, (EXISTS(SELECT 1 FROM departments WHERE parent_id=dep.id)) AS has_more_children
            FROM dep
            ORDER BY created_at
            """;
        using var connection = await _dbConnectionFactory.GetConnectionAsync(cancellationToken);
        int size = query.Request.Size!.Value;
        int page = query.Request.Page!.Value;

        var sqlParams = new { parent_id = query.ParentId, offset = (page - 1) * size, limit = size, };
        var result = await connection.QueryAsync<DepartmentBaseDto>(sqlQuery, sqlParams);
        _logger.LogInformation("Departments received");
        return result.ToList();
    }
}