using Devisions.Application.Abstractions;
using Devisions.Application.Departments.CreateDepartment;
using Devisions.Application.Departments.UpdateLocations;
using Devisions.Contracts.Departments;
using Devisions.Web.EndPointResults;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController(ILogger<DepartmentController> logger) : ControllerBase
{
    [HttpPost]
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
    [Route("{departmentId:Guid}/locations")]
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
}