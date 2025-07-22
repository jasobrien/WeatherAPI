using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeatherAPI.Controllers;
using WeatherAPI.Services;
using WeatherAPI.Models;

namespace WeatherAPI.UnitTests.Controllers;

public class WeatherForecastControllerTests
{
    private readonly Mock<IWeatherService> _mockWeatherService;
    private readonly Mock<ILogger<WeatherForecastController>> _mockLogger;
    private readonly WeatherForecastController _controller;

    public WeatherForecastControllerTests()
    {
        _mockWeatherService = new Mock<IWeatherService>();
        _mockLogger = new Mock<ILogger<WeatherForecastController>>();
        _controller = new WeatherForecastController(_mockWeatherService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task getForecast_whenServiceReturnsData_shouldReturnOkWithForecasts()
    {
        // Arrange
        var expectedForecasts = new List<WeatherForecast>
        {
            new() { hour = 1, temperatureC = 20, rainfallMm = 2.5 },
            new() { hour = 2, temperatureC = 18, rainfallMm = 0.0 }
        };
        _mockWeatherService.Setup(s => s.getForecastAsync()).ReturnsAsync(expectedForecasts);

        // Act
        var result = await _controller.getForecast();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<WeatherForecast>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var actualForecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
        Assert.Equal(expectedForecasts, actualForecasts);
    }

    [Fact]
    public async Task getForecast_whenServiceThrowsException_shouldReturnInternalServerError()
    {
        // Arrange
        _mockWeatherService.Setup(s => s.getForecastAsync()).ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.getForecast();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<WeatherForecast>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task getForecast_shouldLogInformation()
    {
        // Arrange
        var expectedForecasts = new List<WeatherForecast>();
        _mockWeatherService.Setup(s => s.getForecastAsync()).ReturnsAsync(expectedForecasts);

        // Act
        await _controller.getForecast();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Weather forecast requested")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task getForecast_whenServiceThrowsException_shouldLogError()
    {
        // Arrange
        var exception = new Exception("Service error");
        _mockWeatherService.Setup(s => s.getForecastAsync()).ThrowsAsync(exception);

        // Act
        await _controller.getForecast();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while retrieving weather forecast")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
