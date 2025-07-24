# Weather API

A modern .NET 9.0 Web API that provides 24-hour weather forecasts with temperature and rainfall data.

## 🚀 Features

- **24-hour weather forecasts** with hourly temperature and rainfall data
- **Clean architecture** with separated concerns (Controllers, Services, Models)
- **Comprehensive testing** with unit tests and BDD acceptance tests
- **OpenAPI/Swagger documentation** with interactive API explorer
- **Dependency injection** for loose coupling and testability
- **Structured logging** for observability
- **camelCase naming conventions** throughout the codebase

## 📁 Project Structure

```
WeatherAPI/
├── src/
│   └── WeatherAPI/                    # Main API project
│       ├── Controllers/               # API controllers
│       │   └── WeatherForecastController.cs
│       ├── Models/                    # Data models
│       │   └── WeatherForecast.cs
│       ├── Services/                  # Business logic services
│       │   └── WeatherService.cs
│       └── Program.cs                 # Application startup
├── test/
│   ├── WeatherAPI.UnitTests/          # Unit tests with Xunit & Moq
│   │   ├── Controllers/
│   │   └── Services/
│   └── WeatherAPI.AcceptanceTests/    # BDD tests with Reqnroll
│       ├── Features/
│       └── StepDefinitions/
└── WeatherAPI.sln                     # Solution file
```

## 🛠 Technologies Used

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API framework
- **Swashbuckle/OpenAPI** - API documentation
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for unit tests
- **Reqnroll** - BDD testing framework (formerly SpecFlow)
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing

## 🎯 API Endpoints

### GET `/api/WeatherForecast`

Returns a 24-hour weather forecast with hourly data.

**Response Example:**

```json
[
  {
    "hour": 1,
    "temperatureC": 22,
    "rainfallMm": 2.5,
    "temperatureF": 71
  },
  {
    "hour": 2,
    "temperatureC": 20,
    "rainfallMm": 0.0,
    "temperatureF": 68
  }
  // ... 22 more hours
]
```

**Response Fields:**

- `hour`: Hour number (1-24)
- `temperatureC`: Temperature in Celsius (-5 to 34°C)
- `rainfallMm`: Rainfall in millimeters (0.0 to 10.0mm)
- `temperatureF`: Temperature in Fahrenheit (calculated)

## 🚦 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running the API

1. **Clone and navigate to the project:**

   ```bash
   cd /Users/jamesobrien/dev/dotnet/WeatherAPI
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

3. **Build the project:**

   ```bash
   dotnet build
   ```

4. **Run the API:**

   ```bash
   dotnet run --project src/WeatherAPI
   ```

5. **Access the API:**
   - API: `http://localhost:5555/api/WeatherForecast`
   - Swagger UI: `http://localhost:5555/swagger`

### Running Tests

**Run all tests:**

```bash
dotnet test
```

> ✅ **All tests are working**: 9 unit tests + 3 acceptance tests = 12 total tests passing

**Run unit tests only:**

```bash
dotnet test test/WeatherAPI.UnitTests
```

**Run acceptance tests only:**

```bash
dotnet test test/WeatherAPI.AcceptanceTests
```

## 🧪 Testing Strategy

### Unit Tests (`WeatherAPI.UnitTests`)

- **Framework**: xUnit + Moq
- **Coverage**: Controllers and Services
- **Approach**: Isolated unit testing with mocked dependencies

**Test Categories:**

- Controller behavior testing
- Service logic validation
- Error handling verification
- Logging verification

### Acceptance Tests (`WeatherAPI.AcceptanceTests`)

- **Framework**: Reqnroll (BDD) + xUnit
- **Coverage**: End-to-end API behavior
- **Approach**: Behavior-driven development with feature files

**Test Scenarios:**

- 24-hour forecast retrieval
- HTTP status code validation
- JSON response format verification
- Data range validation

## 📋 Coding Standards

### Naming Conventions

- **camelCase** for all JSON properties and method parameters
- **PascalCase** for classes, methods, and public properties
- **Descriptive method names** following `verbNoun` pattern (e.g., `getForecastAsync`)

### Architecture Principles

- **Single Responsibility Principle** - Each class has one reason to change
- **Dependency Inversion** - Depend on abstractions, not concretions
- **Separation of Concerns** - Controllers, Services, and Models are separated
- **Async/Await** - All I/O operations are asynchronous

## 🔧 Configuration

The API uses standard ASP.NET Core configuration:

- `appsettings.json` for general settings
- `appsettings.Development.json` for development overrides
- Environment variables for production settings

## 📝 Logging

The API includes structured logging using the built-in .NET logging framework:

- Information logs for normal operations
- Error logs for exceptions with full stack traces
- All logs include context about the operation being performed

## 🚀 Deployment

The API is ready for containerization and cloud deployment:

- No database dependencies (stateless)
- Health checks ready for implementation
- Configuration externalization support

## 🤝 Contributing

1. Follow the established camelCase coding standards
2. Add unit tests for any new functionality
3. Update BDD scenarios for new features
4. Ensure all tests pass before submitting changes

## 📄 API Documentation

Full API documentation is available via Swagger UI when running the application locally at `/swagger`.
