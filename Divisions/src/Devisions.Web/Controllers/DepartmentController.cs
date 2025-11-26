using Devisions.Application.Abstractions;
using Devisions.Application.Departments.Commands.Create;
using Devisions.Application.Departments.Commands.Move;
using Devisions.Application.Departments.Commands.SoftDelete;
using Devisions.Application.Departments.Commands.UpdateLocations;
using Devisions.Application.Departments.Queries.DepartmentChildren;
using Devisions.Application.Departments.Queries.RootDepartmentsWithChildren;
using Devisions.Application.Departments.Queries.TopDepartments;
using Devisions.Contracts.Departments.Requests;
using Devisions.Contracts.Departments.Responses;
using Devisions.Contracts.Shared;
using Devisions.Web.EndPointResults;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController(ILogger<DepartmentController> logger) : ControllerBase
{
    [HttpPost]
    [Route("/api/departments")]
    public async Task<EndPointResult<Guid>> Create(
        [FromBody] CreateDepartmentRequest createDepartmentRequest,
        [FromServices] ICommandHandler<Guid, CreateDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateDepartmentCommand(createDepartmentRequest);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("Department created: {departmentId}", result.Value);
        return result;
    }

    [HttpPut]
    [Route("/api/{departmentId:Guid}/locations")]
    public async Task<EndPointResult<Guid>> Update(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateLocationsRequest updateLocationsRequest,
        [FromServices] ICommandHandler<Guid, UpdateLocationsCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationsCommand(departmentId, updateLocationsRequest);
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Department with ID:{departmentId} is updated", departmentId);

        return result;
    }

    [HttpPatch]
    [Route("/api/{departmentId:guid}/parent")]
    public async Task<EndPointResult<Guid>> Move(
        [FromRoute] Guid departmentId,
        [FromBody] MoveDepartmentRequest request,
        [FromServices] ICommandHandler<Guid, MoveDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new MoveDepartmentCommand(departmentId, request?.ParentId);
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Department with ID:{departmentId} is moved", departmentId);

        return result;
    }

    [HttpGet]
    [Route("/api/departments/top-positions")]
    public async Task<EndPointResult<IReadOnlyList<DepartmentWithPositionsDto>>> GetTopDepartments(
        [FromServices] IQueryHandler<IReadOnlyList<DepartmentWithPositionsDto>, TopDepartmentsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new TopDepartmentsQuery();
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Top positions is received");

        return result;
    }

    [HttpGet]
    [Route("/api/departments/roots")]
    public async Task<EndPointResult<IReadOnlyList<DepartmentWithChildrenDto>>> GetRootDepartmentsWithChildrenPrefetch(
        [FromQuery] PaginationRequest request,
        [FromQuery] int? prefetch,
        [FromServices]
        IQueryHandler<IReadOnlyList<DepartmentWithChildrenDto>, RootDepartmentsWithChildrenQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new RootDepartmentsWithChildrenQuery(request, prefetch);
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Root departments with children are retrieved");

        return result;
    }

    [HttpGet]
    [Route("/api/departments/{parentId:Guid}/children")]
    public async Task<EndPointResult<IReadOnlyList<DepartmentBaseDto>>> GetDepartmentChildren(
        Guid parentId,
        [FromQuery] PaginationRequest request,
        [FromServices] IQueryHandler<IReadOnlyList<DepartmentBaseDto>, DepartmentChildrenQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new DepartmentChildrenQuery(request, parentId);
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Children of a department {department} are retrieved", parentId);

        return result;
    }

    [HttpDelete]
    [Route("/api/departments/{departmentId:Guid}")]
    public async Task<EndPointResult<Guid>> SoftDelete(
        [FromRoute] Guid departmentId,
        [FromServices] ICommandHandler<Guid, SoftDeleteDepartmentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new SoftDeleteDepartmentCommand(departmentId);
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Department with ID:{departmentId} is deleted", result.Value);

        return result;
    }
}