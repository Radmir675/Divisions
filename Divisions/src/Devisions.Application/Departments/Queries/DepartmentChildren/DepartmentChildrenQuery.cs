using System;
using Devisions.Application.Abstractions;
using Devisions.Contracts.Shared;

namespace Devisions.Application.Departments.Queries.DepartmentChildren;

public record DepartmentChildrenQuery(PaginationRequest Request, Guid ParentId) : IQuery;