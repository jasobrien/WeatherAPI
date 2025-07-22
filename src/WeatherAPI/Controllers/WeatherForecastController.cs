using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Models;
using WeatherAPI.Services;

namespace WeatherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(
        IWeatherService weatherService,
        ILogger<WeatherForecastController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the 24-hour weather forecast including temperature and rainfall data
    /// </summary>
    /// <returns>List of hourly weather forecasts for the next 24 hours</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> getForecast()
    {
        try
        {
            _logger.LogInformation("Weather forecast requested");
            var forecasts = await _weatherService.getForecastAsync();
            return Ok(forecasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving weather forecast");
            return StatusCode(500, "An error occurred while retrieving the weather forecast");
        }
    }
}
