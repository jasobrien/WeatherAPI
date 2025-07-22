# Specmatic Configuration for Weather API

This document explains the Specmatic configuration setup for the Weather API project, focusing on contract testing and stub server configuration.

## 📁 Configuration Files

### 1. Main Configuration (`specmatic.yaml`)

Located in the project root, this file contains the overall Specmatic configuration including:

- Project metadata and source control integration
- Contract and stub server settings
- Environment-specific configurations
- Feature flags and security settings

### 2. Test-Specific Configuration (`test/WeatherAPI.SpecmaticTests/contract/specmatic.yaml`)

This focused configuration file addresses the specific stub matching issues:

- Explicit stub routing rules
- Request matching strategies
- Fallback behavior for unmatched requests
- Debug logging settings

## 🎯 Key Configuration Features

### Stub Matching Strategy

```yaml
# Strict stub matching - use stubs over contract generation
strict: true
disable-examples: true
preload: true

# Request matching priority
priority:
  - query-parameters # Match by query parameters first
  - headers # Then by headers
  - path # Then by path
  - method # Finally by method
```

### Specific Route Configuration

The configuration includes explicit routing rules for each endpoint:

- **Basic weather forecast**: Matches `?basic=true` query parameter
- **London weather**: Matches `?location=London` query parameter
- **Error scenarios**: Matches specific headers like `X-Test-Scenario`
- **Invalid endpoints**: Returns proper 404 responses

### Enhanced Container Setup

The `SpecmaticStubServer.cs` has been updated with:

- Configuration file support (`--config /app/specmatic.yaml`)
- Strict mode enforcement (`--strict`)
- Enhanced logging for debugging
- Configuration validation on startup

## 🗂️ Stub Files Structure

Current stub files in `test/WeatherAPI.SpecmaticTests/contract/stubs/`:

| File                               | Endpoint               | Method | Conditions                             | Response              |
| ---------------------------------- | ---------------------- | ------ | -------------------------------------- | --------------------- |
| `weather-forecast-basic.json`      | `/api/WeatherForecast` | GET    | `?basic=true`                          | 200 with 24 forecasts |
| `weather-forecast-london.json`     | `/api/WeatherForecast` | GET    | `?location=London`                     | 200 with London data  |
| `weather-service-unavailable.json` | `/api/WeatherForecast` | GET    | `X-Test-Scenario: service-unavailable` | 503 error             |
| `weather-unauthorized.json`        | `/api/WeatherForecast` | GET    | `X-Test-Scenario: unauthorized`        | 401 error             |
| `weather-bad-request.json`         | `/api/WeatherForecast` | GET    | `?invalid=true`                        | 400 error             |
| `invalid-endpoint-404.json`        | `/api/InvalidEndpoint` | GET    | -                                      | 404 error             |
| `health-check.json`                | `/health`              | GET    | -                                      | 200 health status     |

## 🚀 Usage

### 1. Validate Configuration

```bash
./validate-specmatic-config.sh
```

### 2. Run Tests with New Configuration

```bash
./run-local-ci.sh
```

### 3. Debug Stub Matching Issues

The enhanced configuration includes detailed logging. Check container logs:

```bash
docker logs <container-id>
```

## 🔧 Troubleshooting Stub Matching Issues

### Issue: Specmatic generates responses from contract instead of using stubs

**Solution Applied:**

1. **Strict mode enabled**: `--strict` flag forces Specmatic to use stubs
2. **Contract generation disabled**: `disable-examples: true` prevents automatic generation
3. **Explicit routing**: Each stub has specific matching conditions
4. **Enhanced logging**: Debug information helps identify matching issues

### Issue: Container restarts during dynamic stub testing

**Solution Applied:**

1. **Pre-loading**: All stubs are loaded on startup
2. **Configuration-based routing**: Eliminates need for dynamic restarts
3. **Optimized container setup**: Better resource allocation

### Issue: Query parameter and header matching

**Solution Applied:**

1. **Exact matching**: Query parameters must match exactly
2. **Header-based routing**: Uses `X-Test-Scenario` header for error scenarios
3. **Priority-based matching**: Clear precedence order for request matching

## 📊 Expected Test Results

With this configuration, the following tests should now pass:

✅ **Get weather forecast returns valid contract response**

- Returns 24 items instead of 1
- Uses `weather-forecast-basic.json` stub

✅ **Get weather forecast with specific location**

- Returns London-specific data
- Uses `weather-forecast-london.json` stub

✅ **Handle invalid endpoint requests**

- Returns 404 instead of 400
- Uses `invalid-endpoint-404.json` stub

✅ **Dynamic stub creation and testing**

- Uses predefined stubs instead of dynamic generation
- Eliminates container restart overhead

## 🔍 Configuration Validation

The `validate-specmatic-config.sh` script checks:

- ✅ Configuration file presence and validity
- ✅ Contract file structure
- ✅ Stub file JSON validity and Specmatic structure
- ✅ Docker image availability
- ✅ Common configuration issues

## 🎛️ Environment Variables

The Specmatic container now uses these environment variables:

- `SPECMATIC_STUB_STRICT=true`: Enforces strict stub matching
- `SPECMATIC_LOG_LEVEL=DEBUG`: Enables detailed logging for debugging

## 📖 Additional Resources

- [Specmatic Documentation](https://specmatic.in/documentation)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Contract Testing Best Practices](https://pact.io/blog/what_is_contract_testing/)

## 🎯 Next Steps

1. Run the validation script to ensure everything is configured correctly
2. Execute the local CI pipeline to test the new configuration
3. Review the HTML test reports for detailed results
4. Monitor container logs for any remaining stub matching issues
