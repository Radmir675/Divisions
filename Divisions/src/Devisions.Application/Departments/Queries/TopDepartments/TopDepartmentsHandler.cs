using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Application.Services;
using Devisions.Contracts.Departments.Responses;
using Devisions.Contracts.Positions.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.TopDepartments;

public record TopDepartmentsQuery(byte TopDepartmentsCount = 5) : IQuery;

public class TopDepartmentsHandler : IQueryHandler<IReadOnlyList<DepartmentBaseDto>, TopDepartmentsQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly ICacheService _cache;

    public TopDepartmentsHandler(IReadDbContext readDbContext, ICacheService cache)
    {
        _readDbContext = readDbContext;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<DepartmentBaseDto>, Errors>> Handle(
        TopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
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
        TopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var departmentDtos = _readDbContext.DepartmentsRead
            .Include(x => x.DepartmentPositions)
            .Select(d =>
                new DepartmentBaseDto
                {
                    Id = d.Id.Value,
                    Name = d.Name.Name,
                    Identifier = d.Identifier.Identify,
                    ParentId = d.ParentId != null ? d.ParentId.Value : null,
                    Path = d.Path.PathValue,
                    Depth = d.Depth,
                    IsActive = d.IsActive,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt,
                    Positions = (from p in _readDbContext.PositionsRead
                            join dp in _readDbContext.DepartmentPositionsRead
                                on new { PositionId = p.Id, DepartmentId = d.Id, } equals new
                                {
                                    dp.PositionId, dp.DepartmentId,
                                }
                                into departmentsPositionsGroup
                            from linkedPosition in departmentsPositionsGroup
                            select new PositionInfoDto { Id = p.Id.Value, Name = p.Name.Value })
                        .OrderBy(x => x.Name)
                        .ToList(),
                });
        var result = await departmentDtos
            .OrderByDescending(dp => dp.Positions.Count)
            .Take(query.TopDepartmentsCount)
            .ToListAsync(cancellationToken);
        return result;
    }
}