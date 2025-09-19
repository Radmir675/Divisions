using Microsoft.AspNetCore.Mvc;

namespace Devisions.WEB.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
   [HttpGet]
   public IActionResult Get()
   {
      return Ok("Divisions API");
   }
}