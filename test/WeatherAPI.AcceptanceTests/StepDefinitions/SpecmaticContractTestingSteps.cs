using System.Net;
using System.Text.Json;
using Reqnroll;
using WeatherAPI.AcceptanceTests.Hooks;
using WeatherAPI.AcceptanceTests.Infrastructure;
using Xunit;

namespace WeatherAPI.AcceptanceTests.StepDefinitions;

[Binding]
public class SpecmaticContractTestingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _lastResponse;
    private readonly List<HttpResponseMessage> _concurrentResponses = new();

    public SpecmaticContractTestingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"the Specmatic stub server is running")]
    public void GivenTheSpecmaticStubServerIsRunning()
    {
        // Verify the stub server is available
        var stubServer = SpecmaticContainerHooks.GetStubServer();
        var httpClient = SpecmaticContainerHooks.GetHttpClient();

        Assert.NotNull(stubServer);
        Assert.NotNull(httpClient);
        Assert.True(stubServer.IsRunning);

        Console.WriteLine($"✅ Specmatic stub server is running at: {stubServer.BaseUrl}");
    }

    [Given(@"the weather forecast contract is loaded")]
    public async Task GivenTheWeatherForecastContractIsLoaded()
    {
        var stubServer = SpecmaticContainerHooks.GetStubServer();

        // Ensure basic stubs are available
        await CreateBasicStubs(stubServer);

        Console.WriteLine("📋 Weather forecast contract loaded successfully");
    }

    [Given(@"I have a custom stub for location ""(.*)""")]
    public async Task GivenIHaveACustomStubForLocation(string location)
    {
        var stubServer = SpecmaticContainerHooks.GetStubServer();

        // Create location-specific stub using query parameters
        var locationStub = StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithRequestQueryParameter("location", location)
            .WithResponse(200)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(GenerateLocationSpecificWeatherData(location))
            .Build();

        await CreateTemporaryStub($"location_{location.ToLower()}", locationStub, stubServer);

        _scenarioContext["location"] = location;
        Console.WriteLine($"🌍 Created custom stub for location: {location}");
    }

    private static object[] GenerateLocationSpecificWeatherData(string location)
    {
        var random = new Random(location.GetHashCode()); // Use location-based seed for consistent data
        var forecasts = new object[24];

        for (int i = 1; i <= 24; i++)
        {
            var tempC = location.ToLower() switch
            {
                "london" => random.Next(5, 18),      // London typical temps
                "dubai" => random.Next(25, 45),      // Dubai typical temps  
                "moscow" => random.Next(-15, 5),     // Moscow typical temps
                _ => random.Next(-5, 30)             // Default range
            };

            forecasts[i - 1] = new
            {
                hour = i,
                temperatureC = tempC,
                temperatureF = 32 + (int)(tempC * 1.8),
                rainfallMm = Math.Round(random.NextDouble() * (location == "London" ? 5 : 2), 1),
                location = location
            };
        }

        return forecasts;
    }

    [Given(@"I create a custom stub for extreme weather conditions")]
    public async Task GivenICreateACustomStubForExtremeWeatherConditions()
    {
        var stubServer = SpecmaticContainerHooks.GetStubServer();

        // Create extreme weather stub with distinguishing header
        var extremeWeatherStub = StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithRequestHeader("X-Test-Scenario", "extreme-weather")
            .WithResponse(200)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(new[]
            {
                new
                {
                    hour = 1,
                    temperatureC = -40,
                    temperatureF = -40,
                    rainfallMm = 50.0,
                    conditions = "Extreme blizzard"
                },
                new
                {
                    hour = 2,
                    temperatureC = 55,
                    temperatureF = 131,
                    rainfallMm = 0.0,
                    conditions = "Heat wave"
                }
            })
            .Build();

        await CreateTemporaryStub("extreme_weather", extremeWeatherStub, stubServer);

        _scenarioContext["stub_type"] = "extreme_weather";
        Console.WriteLine("🌪️ Created custom stub for extreme weather conditions");
    }

    [When(@"I request the weather forecast from Specmatic")]
    public async Task WhenIRequestTheWeatherForecastFromSpecmatic()
    {
        var httpClient = SpecmaticContainerHooks.GetHttpClient();

        // Check if this is for the extreme weather test
        if (_scenarioContext.TryGetValue("stub_type", out var stubType) && stubType.ToString() == "extreme_weather")
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Test-Scenario", "extreme-weather");
            _lastResponse = await httpClient.GetAsync("/api/WeatherForecast");
        }
        else
        {
            // Use basic query parameter to match our basic stub
            _lastResponse = await httpClient.GetAsync("/api/WeatherForecast?basic=true");
        }

        _scenarioContext["response"] = _lastResponse;

        Console.WriteLine($"📡 Requested weather forecast, got status: {_lastResponse.StatusCode}");
    }

    [When(@"I request the weather forecast for ""(.*)""")]
    public async Task WhenIRequestTheWeatherForecastForLocation(string location)
    {
        var httpClient = SpecmaticContainerHooks.GetHttpClient();

        _lastResponse = await httpClient.GetAsync($"/api/WeatherForecast?location={location}");
        _scenarioContext["response"] = _lastResponse;

        Console.WriteLine($"📡 Requested weather forecast for {location}, got status: {_lastResponse.StatusCode}");
    }

    [When(@"I request an invalid endpoint ""(.*)""")]
    public async Task WhenIRequestAnInvalidEndpoint(string endpoint)
    {
        var httpClient = SpecmaticContainerHooks.GetHttpClient();

        _lastResponse = await httpClient.GetAsync(endpoint);
        _scenarioContext["response"] = _lastResponse;

        Console.WriteLine($"📡 Requested invalid endpoint {endpoint}, got status: {_lastResponse.StatusCode}");
    }

    [When(@"I make (.*) concurrent requests to the weather forecast endpoint")]
    public async Task WhenIMakeConcurrentRequestsToTheWeatherForecastEndpoint(int requestCount)
    {
        var httpClient = SpecmaticContainerHooks.GetHttpClient();
        var semaphore = new SemaphoreSlim(3, 3); // Limit to 3 concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create tasks with semaphore to control concurrency
            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(MakeThrottledRequest(httpClient, semaphore));
            }

            var responses = await Task.WhenAll(tasks);
            stopwatch.Stop();

            _concurrentResponses.AddRange(responses);
            _scenarioContext["concurrent_responses"] = responses;
            _scenarioContext["concurrent_duration"] = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"📡 Made {requestCount} concurrent requests in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"❌ Concurrent request failed: {ex.Message}");

            // Clean up any successful responses
            foreach (var response in _concurrentResponses)
            {
                response?.Dispose();
            }
            _concurrentResponses.Clear();

            throw;
        }
        finally
        {
            semaphore.Dispose();
        }
    }

    private async Task<HttpResponseMessage> MakeThrottledRequest(HttpClient httpClient, SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        try
        {
            return await httpClient.GetAsync("/api/WeatherForecast");
        }
        finally
        {
            semaphore.Release();
        }
    }

    [When(@"I request the weather forecast with (.*)")]
    public async Task WhenIRequestTheWeatherForecastWithCondition(string condition)
    {
        var httpClient = SpecmaticContainerHooks.GetHttpClient();
        var stubServer = SpecmaticContainerHooks.GetStubServer();

        // For most error scenarios, we need to create specific stubs that override the default behavior
        if (condition != "invalid query parameter")
        {
            await CreateSpecificErrorStub(condition, stubServer);
        }

        var endpoint = condition switch
        {
            "invalid query parameter" => "/api/WeatherForecast?invalid=true&malformed",
            "unauthorized access" => "/api/WeatherForecast",
            "service unavailable" => "/api/WeatherForecast",
            _ => "/api/WeatherForecast"
        };

        // Add appropriate headers for different conditions
        if (condition == "unauthorized access")
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer invalid-token");
        }
        else if (condition == "service unavailable")
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Test-Scenario", "service-unavailable");
        }

        _lastResponse = await httpClient.GetAsync(endpoint);
        _scenarioContext["response"] = _lastResponse;
        _scenarioContext["condition"] = condition;

        Console.WriteLine($"📡 Requested weather forecast with {condition}, got status: {_lastResponse.StatusCode}");
    }

    private async Task CreateSpecificErrorStub(string condition, SpecmaticStubServer stubServer)
    {
        StubDefinition? errorStub = condition switch
        {
            "service unavailable" => StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/WeatherForecast")
                .WithRequestHeader("X-Test-Scenario", "service-unavailable")
                .WithResponse(503)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Service Unavailable", message = "The service is temporarily unavailable." })
                .Build(),

            "unauthorized access" => StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/WeatherForecast")
                .WithRequestHeader("Authorization", "Bearer invalid-token")
                .WithResponse(401)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Unauthorized", message = "Access denied. Please provide valid credentials." })
                .Build(),

            _ => null
        };

        if (errorStub != null)
        {
            await CreateTemporaryStub($"error_{condition.Replace(" ", "_")}", errorStub, stubServer);
        }
    }

    [Then(@"the response should have status code (.*)")]
    public void ThenTheResponseShouldHaveStatusCode(int expectedStatusCode)
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");

        Assert.Equal((HttpStatusCode)expectedStatusCode, response.StatusCode);
        Console.WriteLine($"✅ Status code {expectedStatusCode} verified");
    }

    [Then(@"the response should contain (.*) hourly forecasts")]
    public async Task ThenTheResponseShouldContainHourlyForecasts(int expectedCount)
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var content = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"🔍 Response content: {content}");

        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(forecasts);
        Console.WriteLine($"🔍 Expected: {expectedCount}, Actual: {forecasts.Length}");
        Assert.Equal(expectedCount, forecasts.Length);

        Console.WriteLine($"✅ Found {expectedCount} hourly forecasts as expected");
    }

    [Then(@"each forecast should have required properties")]
    public async Task ThenEachForecastShouldHaveRequiredProperties()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var content = await response.Content.ReadAsStringAsync();

        var forecasts = JsonSerializer.Deserialize<JsonElement[]>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(forecasts);

        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.TryGetProperty("hour", out var hourProp));
            Assert.True(forecast.TryGetProperty("temperatureC", out var tempCProp));
            Assert.True(forecast.TryGetProperty("temperatureF", out var tempFProp));
            Assert.True(forecast.TryGetProperty("rainfallMm", out var rainfallProp));

            // Validate data types and ranges
            Assert.True(hourProp.GetInt32() >= 1 && hourProp.GetInt32() <= 24);
            Assert.True(tempCProp.GetInt32() >= -50 && tempCProp.GetInt32() <= 60);
            Assert.True(rainfallProp.GetDouble() >= 0);
        }

        Console.WriteLine("✅ All forecasts have required properties with valid values");
    }

    [Then(@"the response should match the contract schema")]
    public async Task ThenTheResponseShouldMatchTheContractSchema()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var content = await response.Content.ReadAsStringAsync();

        // Basic schema validation
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType?.MediaType ?? "");

        // Validate JSON structure
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.NotEqual(JsonValueKind.Undefined, json.ValueKind);

        Console.WriteLine("✅ Response matches contract schema");
    }

    [Then(@"the response should still match the contract schema")]
    public async Task ThenTheResponseShouldStillMatchTheContractSchema()
    {
        await ThenTheResponseShouldMatchTheContractSchema();
    }

    [Then(@"the response should contain location-specific data")]
    public async Task ThenTheResponseShouldContainLocationSpecificData()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var content = await response.Content.ReadAsStringAsync();
        var location = _scenarioContext.Get<string>("location");

        Assert.Contains(location, content);

        Console.WriteLine($"✅ Response contains location-specific data for {location}");
    }

    [Then(@"the response should match error contract")]
    public void ThenTheResponseShouldMatchErrorContract()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");

        Assert.False(response.IsSuccessStatusCode);

        Console.WriteLine($"✅ Error response matches contract with status {response.StatusCode}");
    }

    [Then(@"all responses should have status code (.*)")]
    public void ThenAllResponsesShouldHaveStatusCode(int expectedStatusCode)
    {
        Assert.All(_concurrentResponses, response =>
            Assert.Equal((HttpStatusCode)expectedStatusCode, response.StatusCode));

        Console.WriteLine($"✅ All {_concurrentResponses.Count} concurrent responses have status {expectedStatusCode}");
    }

    [Then(@"all responses should match the contract schema")]
    public async Task ThenAllResponsesShouldMatchTheContractSchema()
    {
        foreach (var response in _concurrentResponses)
        {
            Assert.True(response.IsSuccessStatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            Assert.NotEqual(JsonValueKind.Undefined, json.ValueKind);
        }

        Console.WriteLine($"✅ All {_concurrentResponses.Count} concurrent responses match contract schema");
    }

    [Then(@"response times should be reasonable")]
    public void ThenResponseTimesShouldBeReasonable()
    {
        var duration = _scenarioContext.Get<long>("concurrent_duration");
        var requestCount = _concurrentResponses.Count;
        var avgResponseTime = duration / requestCount;

        // Assert reasonable response time (less than 1 second average)
        Assert.True(avgResponseTime < 1000,
            $"Average response time {avgResponseTime}ms is too high");

        Console.WriteLine($"✅ Average response time {avgResponseTime}ms is reasonable");
    }

    [Then(@"the response should contain the extreme weather data")]
    public async Task ThenTheResponseShouldContainTheExtremeWeatherData()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var content = await response.Content.ReadAsStringAsync();

        // Check for extreme values that were stubbed
        Assert.Contains("-40", content);  // Extreme cold
        Assert.Contains("55", content);   // Extreme heat

        Console.WriteLine("✅ Response contains extreme weather data as expected");
    }

    [Then(@"the error response should match the contract")]
    public void ThenTheErrorResponseShouldMatchTheContract()
    {
        var response = _lastResponse ?? throw new InvalidOperationException("No response available");
        var condition = _scenarioContext.Get<string>("condition");

        // Validate error response based on condition
        var expectedStatusCode = condition switch
        {
            "invalid query parameter" => HttpStatusCode.BadRequest,
            "unauthorized access" => HttpStatusCode.Unauthorized,
            "service unavailable" => HttpStatusCode.ServiceUnavailable,
            _ => HttpStatusCode.BadRequest
        };

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Console.WriteLine($"✅ Error response for '{condition}' matches contract with status {response.StatusCode}");
    }

    private async Task CreateBasicStubs(SpecmaticStubServer stubServer)
    {
        var stubsFolder = Path.Combine(stubServer.ContractFolderPath, "stubs");
        Directory.CreateDirectory(stubsFolder);

        // Check if a basic weather forecast stub already exists
        var existingStubPath = Path.Combine(stubsFolder, "weather-forecast-basic.json");
        if (File.Exists(existingStubPath))
        {
            Console.WriteLine("✅ Basic weather forecast stub already exists, skipping creation");
            await CreateErrorScenarioStubs(stubServer);
            return;
        }

        // Create basic weather forecast stub only if it doesn't exist
        var weatherStub = StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithResponse(200)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(GenerateWeatherForecastData(24))
            .Build();

        await CreatePermanentStub("weather-forecast-basic", weatherStub, stubServer);

        // Create error scenario stubs
        await CreateErrorScenarioStubs(stubServer);
    }

    private async Task CreateErrorScenarioStubs(SpecmaticStubServer stubServer)
    {
        // Create stubs for different error scenarios
        var errorStubs = new Dictionary<string, StubDefinition>
        {
            ["weather-bad-request"] = StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/WeatherForecast?invalid=true&malformed")
                .WithResponse(400)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Bad Request", message = "Invalid query parameter provided." })
                .Build(),

            ["weather-unauthorized"] = StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/WeatherForecast")
                .WithResponse(401)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Unauthorized", message = "Access denied. Please provide valid credentials." })
                .Build(),

            ["weather-service-unavailable"] = StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/WeatherForecast")
                .WithResponse(503)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Service Unavailable", message = "The service is temporarily unavailable." })
                .Build(),

            ["invalid-endpoint-404"] = StubDefinitionBuilder.Create()
                .WithRequest("GET", "/api/InvalidEndpoint")
                .WithResponse(404)
                .WithResponseHeader("Content-Type", "application/json")
                .WithResponseBody(new { error = "Not Found", message = "The requested resource was not found." })
                .Build()
        };

        foreach (var (name, stub) in errorStubs)
        {
            await CreatePermanentStub(name, stub, stubServer);
        }

        Console.WriteLine("📋 Created error scenario stubs for testing");
    }

    private async Task CreateTemporaryStub(string name, StubDefinition stub, SpecmaticStubServer stubServer)
    {
        // Create the stub in Specmatic's expected format using http-request/http-response format
        var specmaticStub = new
        {
            httpRequest = new
            {
                method = stub.HttpRequest.Method,
                path = stub.HttpRequest.Path,
                headers = stub.HttpRequest.Headers,
                queryParameters = stub.HttpRequest.QueryParameters
            },
            httpResponse = new
            {
                status = stub.HttpResponse.Status,
                headers = stub.HttpResponse.Headers,
                body = stub.HttpResponse.Body
            }
        };

        var stubJson = JsonSerializer.Serialize(specmaticStub, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower, // This converts to http-request/http-response
            WriteIndented = true
        });

        // Manually replace the property names to use the correct format
        stubJson = stubJson.Replace("\"httpRequest\"", "\"http-request\"")
                          .Replace("\"httpResponse\"", "\"http-response\"");

        var stubFilePath = Path.Combine(stubServer.ContractFolderPath, "stubs", $"temp_{name}.json");
        await File.WriteAllTextAsync(stubFilePath, stubJson);

        Console.WriteLine($"📄 Created temporary stub file: {stubFilePath}");

        // CRITICAL: Restart the Specmatic container to pick up the new stub
        // Specmatic doesn't support hot-reloading of stub files
        Console.WriteLine("🔄 Restarting Specmatic container to load new stub...");
        await SpecmaticContainerHooks.RestartContainer();
        Console.WriteLine("✅ Specmatic container restarted successfully");
    }

    private async Task CreatePermanentStub(string name, StubDefinition stub, SpecmaticStubServer stubServer)
    {
        // Create the stub in Specmatic's expected format
        var specmaticStub = new
        {
            httpRequest = new
            {
                method = stub.HttpRequest.Method,
                path = stub.HttpRequest.Path,
                headers = stub.HttpRequest.Headers,
                queryParameters = stub.HttpRequest.QueryParameters
            },
            httpResponse = new
            {
                status = stub.HttpResponse.Status,
                headers = stub.HttpResponse.Headers,
                body = stub.HttpResponse.Body
            }
        };

        var stubJson = JsonSerializer.Serialize(specmaticStub, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        // Convert to the correct format that Specmatic expects
        stubJson = stubJson.Replace("\"httpRequest\"", "\"http-request\"")
                          .Replace("\"httpResponse\"", "\"http-response\"");

        var stubFilePath = Path.Combine(stubServer.ContractFolderPath, "stubs", $"{name}.json");
        if (!File.Exists(stubFilePath))
        {
            await File.WriteAllTextAsync(stubFilePath, stubJson);
        }
    }

    private static object[] GenerateWeatherForecastData(int hours)
    {
        var random = new Random(42); // Use seed for consistent test data
        var forecasts = new object[hours];

        for (int i = 1; i <= hours; i++)
        {
            var tempC = random.Next(-20, 40);
            forecasts[i - 1] = new
            {
                hour = i,
                temperatureC = tempC,
                temperatureF = 32 + (int)(tempC * 1.8),
                rainfallMm = Math.Round(random.NextDouble() * 10, 1)
            };
        }

        return forecasts;
    }
}
