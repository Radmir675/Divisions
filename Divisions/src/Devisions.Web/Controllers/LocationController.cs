using Devisions.Application.Abstractions;
using Devisions.Application.Locations.CreateLocation;
using Devisions.Contracts.Locations;
using Devisions.Web.EndPointResults;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController(ILogger<LocationController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<EndPointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        CreateLocationDto request,
        CancellationToken cancellationToken)
    {
        throw new Exception("Create location exception");
        var command = new CreateLocationCommand(request);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("Location created: {locationId}", result.Value);

        logger.LogError("Location is not created!");
        return result;
    }
}