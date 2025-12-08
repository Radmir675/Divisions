using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Application.Departments.Queries.RootDepartmentsWithChildren;
using Devisions.Application.Extensions;
using Devisions.Application.Services;
using Devisions.Contracts.Departments.Responses;
using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.DepartmentChildren;

public class DepartmentChildrenHandler : IQueryHandler<IReadOnlyList<DepartmentBaseDto>, DepartmentChildrenQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<DepartmentChildrenQuery> _validator;
    private readonly ICacheService _cache;
    private readonly ILogger<DepartmentChildrenHandler> _logger;

    public DepartmentChildrenHandler(
        IDbConnectionFactory dbConnectionFactory,
        IValidator<DepartmentChildrenQuery> validator,
        ICacheService cache,
        ILogger<DepartmentChildrenHandler> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DepartmentBaseDto>, Errors>> Handle(
        DepartmentChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToErrors();

        DistributedCacheEntryOptions options = new() { SlidingExpiration = TimeSpan.FromMinutes(5) };
        string key = "departments_" + JsonSerializer.Serialize(query);

        var cachedData = await _cache.GetOrSetAsync(
            key,
            options,
            async () => await GetDepartmentsAsync(query, cancellationToken),
            cancellationToken);

        if (cachedData is null)
            return GeneralErrors.NotFound().ToErrors();

        return cachedData;
    }

    private async Task<List<DepartmentBaseDto>> GetDepartmentsAsync(
        DepartmentChildrenQuery query,
        CancellationToken cancellationToken)
    {
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

        var sqlParams = new { parent_id = query.ParentId, offset = (page - 1) * size, limit = size };
        var result = await connection.QueryAsync<DepartmentBaseDto>(sqlQuery, sqlParams);
        _logger.LogInformation("Departments received");
        return result.ToList();
    }
}