# .gitignore Updates for Specmatic Configuration

## ✅ Files Added to .gitignore

### 🗂️ **Test Results and Reports**
- `LocalTestResults/` - Local test execution results
- `**/TestResults/` - Test output directories
- `test-reports/` - Generated test reports
- `*.trx` - MSTest result files
- `test-summary.html` - Custom test summary reports
- `coverage.cobertura.xml` - Coverage reports

### 🔧 **Specmatic Temporary Files**
- `**/temp_*.json` - Temporary stub files created during testing
- `**/dynamic_*.json` - Dynamically generated stub files
- `**/generated_*.json` - Auto-generated files
- `specmatic-main.yaml` - Backup/duplicate configuration files
- `**/weather-api-contract_examples/` - Auto-generated example files

### 📝 **Debug and Development Files**
- `specmatic.log` - Specmatic logging output
- `debug-specmatic.sh` - Debug scripts
- `manual-test-*.sh` - Manual testing scripts
- `*.sh.log` - Shell script log outputs

### 🐳 **Container and Tool Output**
- `.specmatic/` - Specmatic tool directories
- `**/container-logs/` - Docker container logs
- `**/reportgenerator/` - ReportGenerator output
- `**/htmlcov/` - HTML coverage reports

## ✅ Files That Should NOT Be Ignored

### 📋 **Configuration Files** (tracked in git)
- `specmatic.yaml` - Main Specmatic configuration
- `test/WeatherAPI.SpecmaticTests/contract/specmatic.yaml` - Test-specific configuration
- `weather-api-contract.yaml` - OpenAPI contract definition

### 🗂️ **Stub Files** (tracked in git)
- `test/WeatherAPI.SpecmaticTests/contract/stubs/*.json` - Predefined stub files
- All manually created stub definitions

### 🧪 **Test Definitions** (tracked in git)
- `*.feature` files - Gherkin test scenarios
- `*Steps.cs` files - Step definition implementations

### 🚀 **Scripts** (tracked in git)
- `run-*.sh` - CI/CD execution scripts
- `validate-*.sh` - Validation scripts

## 🎯 **Benefits of This Configuration**

1. **Clean Repository**: Temporary files and test results don't clutter the repo
2. **Faster Clones**: Reduced repository size by excluding generated content
3. **Clear Intent**: Important configuration files are explicitly preserved
4. **Cross-Platform**: Handles macOS, Windows, and Linux development artifacts
5. **CI/CD Friendly**: Test results are excluded but important configs are preserved

## 🔍 **Verification Commands**

```bash
# Check that important files are NOT ignored
git check-ignore specmatic.yaml test/WeatherAPI.SpecmaticTests/contract/specmatic.yaml

# Check that temporary files WOULD be ignored  
git check-ignore LocalTestResults/ test/WeatherAPI.AcceptanceTests/TestResults/

# See what would be ignored in current directory
git status --ignored
```

## 📝 **Notes**

- The `specmatic-main.yaml` file is a duplicate and will be ignored (can be safely removed)
- All manually created stub files in `/stubs/` directory are preserved
- Test execution artifacts are excluded but test definitions are preserved
- Debug scripts are ignored but main execution scripts are preserved
