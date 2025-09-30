using Devisions.Application.Locations;
using Devisions.Contracts;
using Devisions.Web.ResponseExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationsService _locationsService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationsService locationsService, ILogger<LocationController> logger)
    {
        _locationsService = locationsService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var result = await _locationsService.CreateAsync(dto, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToResponse();

        _logger.LogInformation("Location created: {locationId}", result.Value);
        return Ok(result.Value);
    }
}