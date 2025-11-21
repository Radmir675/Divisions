using Devisions.Application.Abstractions;
using Devisions.Contracts.Shared;

namespace Devisions.Application.Departments.Queries.RootDepartmentsWithChildren;

public record RootDepartmentsWithChildrenQuery : IQuery
{
    public PaginationRequest Request { get; init; } = null!;
    public int? Prefetch { get; init; } = 3;

    public RootDepartmentsWithChildrenQuery(PaginationRequest Request, int? Prefetch)
    {
        this.Request = Request;
        if (Prefetch.HasValue)
        {
            this.Prefetch = Prefetch;
        }
    }
}