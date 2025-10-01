using Devisions.Application.Locations;
using Devisions.Contracts.Locations;
using Devisions.Web.EndPointResults;
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
    public async Task<EndPointResult<Guid>> Create(CreateLocationDto dto, CancellationToken cancellationToken)
    {
        var result = await _locationsService.CreateAsync(dto, cancellationToken);

        if (result.IsSuccess)
            _logger.LogInformation("Location created: {locationId}", result.Value);
        return result;
    }
}