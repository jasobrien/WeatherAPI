using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WeatherAPI.Services;
using WeatherAPI.Models;

namespace WeatherAPI.UnitTests.Services;

public class WeatherServiceTests
{
    private readonly Mock<ILogger<WeatherService>> _mockLogger;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _mockLogger = new Mock<ILogger<WeatherService>>();
        _weatherService = new WeatherService(_mockLogger.Object);
    }

    [Fact]
    public async Task getForecastAsync_shouldReturnTwentyFourForecasts()
    {
        // Act
        var result = await _weatherService.getForecastAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(24, result.Count());
    }

    [Fact]
    public async Task getForecastAsync_shouldReturnForecastsWithValidHours()
    {
        // Act
        var result = await _weatherService.getForecastAsync();
        var forecasts = result.ToList();

        // Assert
        for (int i = 0; i < forecasts.Count; i++)
        {
            Assert.Equal(i + 1, forecasts[i].hour);
        }
    }

    [Fact]
    public async Task getForecastAsync_shouldReturnForecastsWithValidTemperatureRanges()
    {
        // Act
        var result = await _weatherService.getForecastAsync();

        // Assert
        foreach (var forecast in result)
        {
            Assert.InRange(forecast.temperatureC, -5, 34);
        }
    }

    [Fact]
    public async Task getForecastAsync_shouldReturnForecastsWithValidRainfallRanges()
    {
        // Act
        var result = await _weatherService.getForecastAsync();

        // Assert
        foreach (var forecast in result)
        {
            Assert.InRange(forecast.rainfallMm, 0.0, 10.0);
        }
    }

    [Fact]
    public async Task getForecastAsync_shouldLogInformation()
    {
        // Act
        await _weatherService.getForecastAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generating 24-hour weather forecast")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
