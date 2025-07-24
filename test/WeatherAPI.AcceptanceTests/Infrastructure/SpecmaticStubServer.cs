using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;

namespace WeatherAPI.AcceptanceTests.Infrastructure;

/// <summary>
/// Simplified Specmatic stub server for acceptance tests
/// </summary>
public class SpecmaticStubServer : IDisposable
{
    private readonly ILogger<SpecmaticStubServer> _logger;
    private readonly int _port;
    private IContainer? _specmaticContainer;
    private bool _isStarted;
    private bool _disposed;

    public SpecmaticStubServer(int port = 9000, string? contractFolderPath = null, ILogger<SpecmaticStubServer>? logger = null)
    {
        _logger = logger ?? CreateDefaultLogger();
        _port = port;
        ContractFolderPath = contractFolderPath ?? Path.Combine(Directory.GetCurrentDirectory(), "contract");
        _isStarted = false;
    }

    public string BaseUrl => $"http://localhost:{_port}";
    public bool IsRunning => _isStarted && _specmaticContainer != null && _specmaticContainer.State == TestcontainersStates.Running;
    public string ContractFolderPath { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
        {
            _logger.LogWarning("Specmatic stub server is already started");
            return;
        }

        try
        {
            await ValidateConfigurationAsync();
            EnsureContractFolderExists();
            LoadStubFiles();
            await StartSpecmaticContainer(cancellationToken);
            await WaitForServerToStart(cancellationToken);

            _isStarted = true;
            _logger.LogInformation("Specmatic stub server started successfully on {BaseUrl}", BaseUrl);

            // Log debug info for troubleshooting
            var debugInfo = await GetStubMatchingDebugInfoAsync();
            _logger.LogDebug("Specmatic Debug Info: {DebugInfo}", debugInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Specmatic stub server");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted || _specmaticContainer == null)
        {
            _logger.LogWarning("Specmatic stub server is not running");
            return;
        }

        try
        {
            if (_specmaticContainer.State == TestcontainersStates.Running)
            {
                await _specmaticContainer.StopAsync(cancellationToken);
            }

            _isStarted = false;
            _logger.LogInformation("Specmatic stub server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Specmatic stub server");
            throw;
        }
    }

    public IEnumerable<string> LoadStubFiles()
    {
        var stubsPath = Path.Combine(ContractFolderPath, "stubs");

        if (!Directory.Exists(stubsPath))
        {
            _logger.LogInformation("Stubs directory does not exist: {Path}", stubsPath);
            return Array.Empty<string>();
        }

        var stubFiles = Directory.GetFiles(stubsPath, "*.json");

        foreach (var stubFile in stubFiles)
        {
            _logger.LogDebug("Loaded stub file: {File}", stubFile);
        }

        _logger.LogInformation("Loaded {Count} valid stub files from {Path}", stubFiles.Length, ContractFolderPath);
        return stubFiles;
    }

    public async Task<(string stdout, string stderr)> GetContainerLogsAsync()
    {
        if (_specmaticContainer == null)
            return ("", "Container not available");

        try
        {
            var logs = await _specmaticContainer.GetLogsAsync();
            return (logs.Stdout, logs.Stderr);
        }
        catch (Exception ex)
        {
            return ("", $"Error getting logs: {ex.Message}");
        }
    }

    private Task EnsureContractFolderExists()
    {
        Directory.CreateDirectory(ContractFolderPath);

        var stubsPath = Path.Combine(ContractFolderPath, "stubs");
        Directory.CreateDirectory(stubsPath);

        _logger.LogInformation("Contract folder ensured at: {Path}", ContractFolderPath);
        return Task.CompletedTask;
    }

    private async Task StartSpecmaticContainer(CancellationToken cancellationToken)
    {
        try
        {
            var contractPath = Path.GetFullPath(ContractFolderPath);
            var stubsPath = Path.Combine(contractPath, "stubs");

            _logger.LogInformation("Starting Specmatic container with contract: {Contract} and stubs: {Stubs}",
                Path.Combine(contractPath, "weather-api-contract.yaml"), stubsPath);

            _specmaticContainer = new ContainerBuilder()
                .WithImage("specmatic/specmatic:latest")
                .WithPortBinding(_port, 9000)
                .WithBindMount(contractPath, "/app")
                .WithCommand("stub", "/app/weather-api-contract.yaml",
                    "--port", "9000",
                    "--data", "/app/stubs",
                    "--config", "/app/specmatic.yaml",
                    "--strict",
                    "--host", "0.0.0.0")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(9000))
                .WithEnvironment("SPECMATIC_STUB_STRICT", "true")
                .WithEnvironment("SPECMATIC_LOG_LEVEL", "DEBUG")
                .Build();

            await _specmaticContainer.StartAsync(cancellationToken);
            _logger.LogInformation("Specmatic container started successfully with ID: {Id}", _specmaticContainer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Specmatic container");
            throw;
        }
    }

    private async Task WaitForServerToStart(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 30;
        const int delayMs = 1000;

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{BaseUrl}/actuator/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Specmatic stub server is ready after {Attempts} attempts", attempt);
                    return;
                }
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogDebug("Health check attempt {Attempt} failed: {Error}", attempt, ex.Message);
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new TimeoutException($"Specmatic stub server failed to start within {maxAttempts} seconds");
    }

    private static ILogger<SpecmaticStubServer> CreateDefaultLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        return loggerFactory.CreateLogger<SpecmaticStubServer>();
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_specmaticContainer != null)
            {
                if (_specmaticContainer.State == TestcontainersStates.Running)
                {
                    _specmaticContainer.StopAsync().GetAwaiter().GetResult();
                }
                _specmaticContainer.DisposeAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during disposal");
        }
        finally
        {
            _disposed = true;
        }
    }

    public async Task ValidateConfigurationAsync()
    {
        var contractPath = Path.GetFullPath(ContractFolderPath);
        var contractFile = Path.Combine(contractPath, "weather-api-contract.yaml");
        var configFile = Path.Combine(contractPath, "specmatic.yaml");
        var stubsPath = Path.Combine(contractPath, "stubs");

        _logger.LogInformation("Validating Specmatic configuration...");

        // Check contract file
        if (!File.Exists(contractFile))
        {
            throw new FileNotFoundException($"Contract file not found: {contractFile}");
        }
        _logger.LogInformation("✅ Contract file found: {File}", contractFile);

        // Check configuration file
        if (!File.Exists(configFile))
        {
            _logger.LogWarning("⚠️ Configuration file not found: {File}", configFile);
        }
        else
        {
            _logger.LogInformation("✅ Configuration file found: {File}", configFile);
        }

        // Check stubs directory and files
        if (!Directory.Exists(stubsPath))
        {
            throw new DirectoryNotFoundException($"Stubs directory not found: {stubsPath}");
        }

        var stubFiles = Directory.GetFiles(stubsPath, "*.json");
        _logger.LogInformation("📁 Stubs directory: {Path}", stubsPath);
        _logger.LogInformation("📋 Found {Count} stub files:", stubFiles.Length);

        foreach (var stubFile in stubFiles)
        {
            var fileName = Path.GetFileName(stubFile);
            try
            {
                var content = await File.ReadAllTextAsync(stubFile);
                JsonDocument.Parse(content); // Validate JSON structure
                _logger.LogInformation("  ✅ {File} - Valid JSON", fileName);
            }
            catch (JsonException ex)
            {
                _logger.LogError("  ❌ {File} - Invalid JSON: {Error}", fileName, ex.Message);
                throw new InvalidDataException($"Invalid JSON in stub file {fileName}: {ex.Message}");
            }
        }

        _logger.LogInformation("✅ Configuration validation completed successfully");
    }

    public async Task<string> GetStubMatchingDebugInfoAsync()
    {
        if (_specmaticContainer == null)
            return "Container not available";

        try
        {
            var logs = await _specmaticContainer.GetLogsAsync();
            var debugInfo = new StringBuilder();
            debugInfo.AppendLine("=== Specmatic Container Debug Info ===");
            debugInfo.AppendLine($"Container ID: {_specmaticContainer.Id}");
            debugInfo.AppendLine($"Container State: {_specmaticContainer.State}");
            debugInfo.AppendLine($"Base URL: {BaseUrl}");
            debugInfo.AppendLine();
            debugInfo.AppendLine("=== Container Logs (Stdout) ===");
            debugInfo.AppendLine(logs.Stdout);
            debugInfo.AppendLine();
            debugInfo.AppendLine("=== Container Logs (Stderr) ===");
            debugInfo.AppendLine(logs.Stderr);

            return debugInfo.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting debug info: {ex.Message}";
        }
    }
}

/// <summary>
/// Stub definition for HTTP request/response pairs that matches Specmatic format
/// </summary>
public class StubDefinition
{
    public SpecmaticHttpRequest HttpRequest { get; set; } = new();
    public SpecmaticHttpResponse HttpResponse { get; set; } = new();
}

public class SpecmaticHttpRequest
{
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public Dictionary<string, string> QueryParameters { get; set; } = new();
}

public class SpecmaticHttpResponse
{
    public int Status { get; set; } = 200;
    public Dictionary<string, string> Headers { get; set; } = new();
    public object? Body { get; set; }
}

/// <summary>
/// Builder for creating stub definitions
/// </summary>
public class StubDefinitionBuilder
{
    private readonly StubDefinition _stub = new();

    public static StubDefinitionBuilder Create() => new();

    public StubDefinitionBuilder WithRequest(string method, string path)
    {
        _stub.HttpRequest.Method = method;
        _stub.HttpRequest.Path = path;
        return this;
    }

    public StubDefinitionBuilder WithRequestHeader(string name, string value)
    {
        _stub.HttpRequest.Headers[name] = value;
        return this;
    }

    public StubDefinitionBuilder WithRequestQueryParameter(string name, string value)
    {
        _stub.HttpRequest.QueryParameters[name] = value;
        return this;
    }

    public StubDefinitionBuilder WithResponse(int statusCode)
    {
        _stub.HttpResponse.Status = statusCode;
        return this;
    }

    public StubDefinitionBuilder WithResponseHeader(string name, string value)
    {
        _stub.HttpResponse.Headers[name] = value;
        return this;
    }

    public StubDefinitionBuilder WithResponseBody(object body)
    {
        _stub.HttpResponse.Body = body;
        return this;
    }

    public StubDefinition Build() => _stub;
}

/// <summary>
/// Common stub definitions for testing
/// </summary>
public static class CommonStubs
{
    public static StubDefinition HealthCheck()
    {
        return StubDefinitionBuilder.Create()
            .WithRequest("GET", "/health")
            .WithResponse(200)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(new { status = "UP" })
            .Build();
    }

    public static StubDefinition WeatherForecastSuccess()
    {
        var forecasts = Enumerable.Range(1, 24).Select(hour => new
        {
            hour,
            temperatureC = Random.Shared.Next(-20, 40),
            temperatureF = Random.Shared.Next(-4, 104),
            rainfallMm = Math.Round(Random.Shared.NextDouble() * 10, 1)
        }).ToArray();

        return StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithResponse(200)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(forecasts)
            .Build();
    }

    public static StubDefinition NotFound(string path)
    {
        return StubDefinitionBuilder.Create()
            .WithRequest("GET", path)
            .WithResponse(404)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(new { error = "Not Found", message = $"The requested resource '{path}' was not found." })
            .Build();
    }

    public static StubDefinition BadRequest()
    {
        return StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithResponse(400)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(new { error = "Bad Request", message = "The request is invalid." })
            .Build();
    }

    public static StubDefinition ServerError()
    {
        return StubDefinitionBuilder.Create()
            .WithRequest("GET", "/api/WeatherForecast")
            .WithResponse(500)
            .WithResponseHeader("Content-Type", "application/json")
            .WithResponseBody(new { error = "Internal Server Error", message = "An unexpected error occurred." })
            .Build();
    }
}
