using System.Net;
using System.Text.Json;
using Reqnroll;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using WeatherAPI;
using WeatherAPI.Models;

namespace WeatherAPI.AcceptanceTests.StepDefinitions;

[Binding]
public class WeatherForecastSteps : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private HttpResponseMessage? _response;
    private List<WeatherForecast>? _forecasts;

    public WeatherForecastSteps(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [When(@"I request the weather forecast")]
    public async Task whenIRequestTheWeatherForecast()
    {
        _response = await _client.GetAsync("/api/WeatherForecast");
        var content = await _response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        _forecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(content, options);
    }

    [When(@"I make a GET request to ""(.*)""")]
    public async Task whenIMakeAGETRequestTo(string endpoint)
    {
        _response = await _client.GetAsync(endpoint);
    }

    [Then(@"I should receive 24 hourly forecasts")]
    public void thenIShouldReceive24HourlyForecasts()
    {
        Assert.NotNull(_forecasts);
        Assert.Equal(24, _forecasts.Count);
    }

    [Then(@"each forecast should contain hour number, temperature, and rainfall data")]
    public void thenEachForecastShouldContainHourNumberTemperatureAndRainfallData()
    {
        Assert.NotNull(_forecasts);
        foreach (var forecast in _forecasts)
        {
            Assert.True(forecast.hour > 0);
            // Note: temperatureC and rainfallMm are value types, so NotNull check is not needed
            Assert.True(forecast.temperatureC >= -50 && forecast.temperatureC <= 50); // Basic range check
            Assert.True(forecast.rainfallMm >= 0); // Rainfall should be non-negative
        }
    }

    [Then(@"the temperature should be between -5 and 34 degrees Celsius")]
    public void thenTheTemperatureShouldBeBetweenMinus5And34DegreesCelsius()
    {
        Assert.NotNull(_forecasts);
        foreach (var forecast in _forecasts)
        {
            Assert.InRange(forecast.temperatureC, -5, 34);
        }
    }

    [Then(@"the rainfall should be non-negative")]
    public void thenTheRainfallShouldBeNonNegative()
    {
        Assert.NotNull(_forecasts);
        foreach (var forecast in _forecasts)
        {
            Assert.True(forecast.rainfallMm >= 0);
        }
    }

    [Then(@"the response status code should be 200")]
    public void thenTheResponseStatusCodeShouldBe200()
    {
        Assert.NotNull(_response);
        Assert.Equal(HttpStatusCode.OK, _response.StatusCode);
    }

    [Then(@"the response should be in JSON format")]
    public void thenTheResponseShouldBeInJSONFormat()
    {
        Assert.NotNull(_response);
        Assert.Equal("application/json", _response.Content.Headers.ContentType?.MediaType);
    }

    [Then(@"the hours should be numbered from 1 to 24")]
    public void thenTheHoursShouldBeNumberedFrom1To24()
    {
        Assert.NotNull(_forecasts);
        var sortedForecasts = _forecasts.OrderBy(f => f.hour).ToList();

        for (int i = 0; i < sortedForecasts.Count; i++)
        {
            Assert.Equal(i + 1, sortedForecasts[i].hour);
        }
    }

    [Then(@"all rainfall values should be between 0 and 10 millimeters")]
    public void thenAllRainfallValuesShouldBeBetween0And10Millimeters()
    {
        Assert.NotNull(_forecasts);
        foreach (var forecast in _forecasts)
        {
            Assert.InRange(forecast.rainfallMm, 0.0, 10.0);
        }
    }
}
