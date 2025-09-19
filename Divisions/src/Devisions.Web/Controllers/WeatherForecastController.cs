using Microsoft.AspNetCore.Mvc;

namespace Devisions.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public ActionResult Get()
    {
        return Ok("Hello World");
    }
}