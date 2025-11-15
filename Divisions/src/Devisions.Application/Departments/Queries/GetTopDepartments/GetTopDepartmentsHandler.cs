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

namespace Devisions.Application.Departments.Queries.GetTopPositions;

public record TopDepartmentsQuery() : IQuery;

public class GetTopDepartmentsHandler : IQueryHandler<IReadOnlyList<DepartmentDto>, TopDepartmentsQuery>
{
    private const byte TOP_DEPARTMENTS_COUNT = 5;
    private readonly IReadDbContext _readDbContext;

    public GetTopDepartmentsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<IReadOnlyList<DepartmentDto>, Errors>> Handle(
        TopDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _readDbContext.DepartmentsRead
            .Include(x => x.DepartmentPositions)
            .Where(x => x.DepartmentPositions.Any())
            .OrderByDescending(dp => dp.DepartmentPositions.Count)
            .Take(TOP_DEPARTMENTS_COUNT)
            .Select(dp =>
                new DepartmentDto
                {
                    Id = dp.Id.Value,
                    Name = dp.Name.Name,
                    Identifier = dp.Identifier.Identify,
                    ParentId = dp.ParentId != null ? dp.ParentId.Value : null,
                    Path = dp.Path.PathValue,
                    Depth = dp.Depth,
                    IsActive = dp.IsActive,
                    CreatedAt = dp.CreatedAt,
                    UpdatedAt = dp.UpdatedAt,
                    DepartmentPositions = _readDbContext.PositionsRead
                        .Where(p => dp.DepartmentPositions
                            .Select(x => x.PositionId)
                            .Contains(p.Id))
                        .Select(p => new PositionInfoDto { Id = p.Id.Value, Name = p.Name.Value, })
                        .OrderBy(x => x.Name)
                        .ToList(),
                })
            .ToListAsync(cancellationToken);

        return result.Any() ? result : GeneralErrors.NotFoundInDatabase().ToErrors();
    }
}