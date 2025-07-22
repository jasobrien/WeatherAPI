using Microsoft.Extensions.Logging;
using Reqnroll;
using WeatherAPI.AcceptanceTests.Infrastructure;

namespace WeatherAPI.AcceptanceTests.Hooks;

/// <summary>
/// Hooks for managing Specmatic container lifecycle during Reqnroll tests
/// </summary>
[Binding]
public class SpecmaticContainerHooks
{
    private static SpecmaticStubServer? _stubServer;
    private static HttpClient? _httpClient;
    private static readonly ILogger<SpecmaticStubServer> _logger = CreateLogger();

    /// <summary>
    /// Start Specmatic container before any tests run
    /// </summary>
    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        try
        {
            Console.WriteLine("🐳 Starting Specmatic container for contract testing...");

            // Create contract folder path
            var contractFolderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "..", "..", "..",
                "WeatherAPI.SpecmaticTests",
                "contract"
            );

            // Ensure contract folder exists and has required files
            EnsureContractFiles(contractFolderPath);

            // Create and start Specmatic stub server
            _stubServer = new SpecmaticStubServer(
                port: 9003, // Use unique port for acceptance tests
                contractFolderPath: Path.GetFullPath(contractFolderPath),
                logger: _logger
            );

            await _stubServer.StartAsync();

            // Create HTTP client for making requests
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_stubServer.BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            Console.WriteLine($"✅ Specmatic container started at: {_stubServer.BaseUrl}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to start Specmatic container: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Stop Specmatic container after all tests complete
    /// </summary>
    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        try
        {
            Console.WriteLine("🛑 Stopping Specmatic container...");

            _httpClient?.Dispose();

            if (_stubServer != null)
            {
                await _stubServer.StopAsync();
                _stubServer.Dispose();
                Console.WriteLine("✅ Specmatic container stopped successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error stopping Specmatic container: {ex.Message}");
        }
    }

    /// <summary>
    /// Clean up any test-specific stubs after each scenario
    /// </summary>
    [AfterScenario]
    public static void AfterScenario()
    {
        try
        {
            // Clean up any dynamic stubs created during the scenario
            if (_stubServer != null)
            {
                var contractPath = _stubServer.ContractFolderPath;
                var stubsPath = Path.Combine(contractPath, "stubs");

                // Remove any temporary stub files (those with 'temp_' prefix)
                if (Directory.Exists(stubsPath))
                {
                    var tempStubs = Directory.GetFiles(stubsPath, "temp_*.json");
                    foreach (var tempStub in tempStubs)
                    {
                        File.Delete(tempStub);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Warning: Could not clean up temporary stubs: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the HTTP client for making requests to Specmatic
    /// </summary>
    public static HttpClient GetHttpClient()
    {
        return _httpClient ?? throw new InvalidOperationException(
            "HTTP client not available. Ensure Specmatic container is running.");
    }

    /// <summary>
    /// Get the stub server instance
    /// </summary>
    public static SpecmaticStubServer GetStubServer()
    {
        return _stubServer ?? throw new InvalidOperationException(
            "Stub server not available. Ensure Specmatic container is running.");
    }

    /// <summary>
    /// Restart Specmatic container to pick up new stub files
    /// Specmatic doesn't support hot-reloading, so we need to restart the container
    /// </summary>
    public static async Task RestartContainer()
    {
        if (_stubServer == null)
        {
            throw new InvalidOperationException("Specmatic container is not running");
        }

        Console.WriteLine("🔄 Restarting Specmatic container for dynamic stub loading...");

        try
        {
            // Stop the current container
            await _stubServer.StopAsync();

            // Start it again (this will pick up new stub files)
            await _stubServer.StartAsync();

            Console.WriteLine("✅ Specmatic container restarted successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to restart Specmatic container: {ex.Message}");
            throw;
        }
    }

    private static Task EnsureContractFiles(string contractFolderPath)
    {
        Directory.CreateDirectory(contractFolderPath);

        var stubsPath = Path.Combine(contractFolderPath, "stubs");
        Directory.CreateDirectory(stubsPath);

        Console.WriteLine($"📋 Contract folder ensured at: {contractFolderPath}");

        return Task.CompletedTask;
    }

    private static ILogger<SpecmaticStubServer> CreateLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole()
                   .SetMinimumLevel(LogLevel.Information);
        });
        return loggerFactory.CreateLogger<SpecmaticStubServer>();
    }
}
