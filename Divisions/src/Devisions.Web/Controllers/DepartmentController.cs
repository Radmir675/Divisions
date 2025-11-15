using Devisions.Application.Abstractions;
using Devisions.Application.Departments.Commands.CreateDepartment;
using Devisions.Application.Departments.Commands.MoveDepartment;
using Devisions.Application.Departments.Commands.UpdateLocations;
using Devisions.Application.Departments.Queries.GetTopPositions;
using Devisions.Contracts.Departments.Requests;
using Devisions.Contracts.Departments.Responses;
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
    [Route("api/departments/top-positions")]
    public async Task<EndPointResult<IEnumerable<TopDepartmentResponse>>> GetTopPositions(
        [FromServices] IQueryHandler<IEnumerable<TopDepartmentResponse>, TopPositionsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new TopPositionsQuery();
        var result = await handler.Handle(query, cancellationToken);
        if (result.IsSuccess)
            logger.LogInformation("Top positions is received");

        return result;
    }
}