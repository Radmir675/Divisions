using Devisions.Application.Abstractions;
using Devisions.Application.Locations.Commands.CreateLocation;
using Devisions.Application.Locations.Queries;
using Devisions.Application.Locations.Queries.GetLocation;
using Devisions.Contracts.Locations.Requests;
using Devisions.Contracts.Locations.Responses;
using Devisions.Web.EndPointResults;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class LocationController(ILogger<LocationController> logger) : ControllerBase
{
    [HttpPost]
    [Route("/api/locations")]
    public async Task<EndPointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("Location created: {locationId}", result.Value);

        return result;
    }

    [HttpGet]
    [Route("/api/locations")]
    public async Task<EndPointResult<IEnumerable<LocationResponse>>> Locations(
        [FromServices] IQueryHandler<IEnumerable<LocationResponse>, GetLocationQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationQuery(request);
        var result = await handler.Handle(query, cancellationToken);

        if (result.IsSuccess)
            logger.LogInformation("Locations got successfully");

        return result;
    }
}