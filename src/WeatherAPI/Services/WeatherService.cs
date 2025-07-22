using WeatherAPI.Models;

namespace WeatherAPI.Services;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> getForecastAsync();
}

public class WeatherService : IWeatherService
{
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<WeatherForecast>> getForecastAsync()
    {
        _logger.LogInformation("Generating 24-hour weather forecast");

        var random = new Random();
        var forecasts = new List<WeatherForecast>();

        for (int i = 1; i <= 24; i++)
        {
            forecasts.Add(new WeatherForecast
            {
                hour = i,
                temperatureC = random.Next(-5, 35),
                rainfallMm = Math.Round(random.NextDouble() * 10, 1)
            });
        }

        // Simulate async operation
        await Task.Delay(10);

        return forecasts;
    }
}
