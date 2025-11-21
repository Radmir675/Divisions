using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Devisions.Application.Abstractions;
using Devisions.Application.Database;
using Devisions.Contracts.Departments.Responses;
using Devisions.Contracts.Positions.Responses;
using Microsoft.EntityFrameworkCore;
using Shared.Errors;

namespace Devisions.Application.Departments.Queries.TopDepartments;

public record TopDepartmentsQuery() : IQuery;

public class TopDepartmentsHandler : IQueryHandler<IReadOnlyList<DepartmentBaseDto>, TopDepartmentsQuery>
{
    private const byte TOP_DEPARTMENTS_COUNT = 5;
    private readonly IReadDbContext _readDbContext;

    public TopDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IReadOnlyList<DepartmentBaseDto>, Errors>> Handle(
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
            .Take(TOP_DEPARTMENTS_COUNT)
            .ToListAsync(cancellationToken);

        return result.Count > 0 ? result : GeneralErrors.NotFoundInDatabase().ToErrors();
    }
}