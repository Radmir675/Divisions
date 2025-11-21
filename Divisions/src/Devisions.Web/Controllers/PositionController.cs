using Devisions.Application.Abstractions;
using Devisions.Application.Positions.Create;
using Devisions.Contracts.Positions.Requests;
using Devisions.Web.EndPointResults;
using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class PositionController : ControllerBase
{
    private readonly ILogger<PositionController> _logger;

    public PositionController(ILogger<PositionController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("/api/positions")]
    public async Task<EndPointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreatePositionCommand> handler,
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand(request);
        var result = await handler.Handle(command, cancellationToken);
        if (result.IsSuccess)
            _logger.LogInformation("Position created with id: {id}", result.Value);

        return result;
    }
}