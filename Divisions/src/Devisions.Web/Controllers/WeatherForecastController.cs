using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        this._logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {
        return Ok("Hello World");
    }
}