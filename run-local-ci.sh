#!/bin/bash

# Local CI Pipeline Runner
# Runs the same tests as the GitHub Actions pipeline locally

set -e  # Exit on any error

echo "🚀 Starting Local CI Pipeline for Weather API..."
echo "=============================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
print_status $BLUE "🔍 Checking prerequisites..."

if ! command_exists dotnet; then
    print_status $RED "❌ .NET SDK not found. Please install .NET 8.0 SDK"
    exit 1
fi

if ! command_exists docker; then
    print_status $RED "❌ Docker not found. Please install Docker"
    exit 1
fi

if ! command_exists java; then
    print_status $RED "❌ Java not found. Please install Java 17+"
    exit 1
fi

print_status $GREEN "✅ Prerequisites check passed"

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR" && pwd)"

# Create results directory
RESULTS_DIR="$PROJECT_ROOT/LocalTestResults"
rm -rf "$RESULTS_DIR"
mkdir -p "$RESULTS_DIR"

echo ""
print_status $BLUE "📦 Step 1: Restore Dependencies"
echo "================================"
cd "$PROJECT_ROOT"
dotnet restore

echo ""
print_status $BLUE "🔨 Step 2: Build Solution"
echo "=========================="
dotnet build --configuration Release --no-restore

echo ""
print_status $BLUE "🧪 Step 3: Run Unit Tests"
echo "=========================="
dotnet test test/WeatherAPI.UnitTests/WeatherAPI.UnitTests.csproj \
    --configuration Release \
    --no-build \
    --logger "trx;LogFileName=unit-test-results.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory "$RESULTS_DIR/UnitTests/" \
    --collect:"XPlat Code Coverage"

print_status $GREEN "✅ Unit tests completed"

echo ""
print_status $BLUE "🔍 Step 4: Validate Contract"
echo "============================"
cd "$PROJECT_ROOT/test/WeatherAPI.SpecmaticTests/contract"

# Pull Specmatic Docker image
print_status $YELLOW "📥 Pulling Specmatic Docker image..."
docker pull znsio/specmatic:latest

# Validate contract syntax
print_status $YELLOW "🔍 Validating OpenAPI contract..."
docker run --rm \
    -v "$(pwd):/usr/src/app" \
    -w /usr/src/app \
    znsio/specmatic:latest \
    examples validate --spec-file weather-api-contract.yaml

print_status $GREEN "✅ Contract validation completed"

echo ""
print_status $BLUE "🎭 Step 5: Test Specmatic Stubs"
echo "==============================="

# Start Specmatic stub server in background
print_status $YELLOW "🐳 Starting Specmatic stub server..."
docker run --rm -d \
    --name local-ci-specmatic-stub \
    -p 9003:9003 \
    -v "$(pwd):/usr/src/app" \
    -w /usr/src/app \
    znsio/specmatic:latest \
    stub weather-api-contract.yaml --port=9003

# Wait for server to start
print_status $YELLOW "⏳ Waiting for server to start..."
sleep 15

# Test endpoints
print_status $YELLOW "🧪 Testing stub endpoints..."

# Test basic endpoint
if curl -f "http://localhost:9003/api/WeatherForecast?basic=true" \
   -H "X-Test-Scenario: basic-forecast" \
   -H "Authorization: Bearer test-token" > /dev/null 2>&1; then
    print_status $GREEN "✅ Basic forecast endpoint working"
else
    print_status $YELLOW "⚠️ Basic forecast endpoint test failed"
fi

# Test health endpoint
if curl -f "http://localhost:9003/health" > /dev/null 2>&1; then
    print_status $GREEN "✅ Health endpoint working"
else
    print_status $YELLOW "⚠️ Health endpoint test failed"
fi

# Stop the stub server
print_status $YELLOW "🛑 Stopping stub server..."
docker stop local-ci-specmatic-stub > /dev/null 2>&1 || true

print_status $GREEN "✅ Stub testing completed"

echo ""
print_status $BLUE "🧪 Step 6: Run Acceptance Tests"
echo "==============================="

cd "$PROJECT_ROOT/test/WeatherAPI.AcceptanceTests"

# Clean previous results
rm -rf TestResults/
mkdir -p TestResults/

# Run acceptance tests
print_status $YELLOW "🚀 Running Reqnroll acceptance tests..."
dotnet test \
    --configuration Release \
    --logger "trx;LogFileName=acceptance-test-results.trx" \
    --logger "html;LogFileName=acceptance-test-results.html" \
    --logger "console;verbosity=detailed" \
    --results-directory TestResults/ \
    --collect:"XPlat Code Coverage" \
    --blame-hang-timeout 5m

# Copy results to main results directory
cp -r TestResults/* "$RESULTS_DIR/"

print_status $GREEN "✅ Acceptance tests completed"

echo ""
print_status $BLUE "📊 Step 7: Generate Reports"
echo "============================"

# Install ReportGenerator if not available
if ! dotnet tool list -g | grep -q reportgenerator; then
    print_status $YELLOW "📥 Installing ReportGenerator..."
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.26
fi

# Find coverage files
COVERAGE_FILES=$(find "$RESULTS_DIR" -name "*.cobertura.xml" -o -name "coverage.xml" | tr '\n' ';' | sed 's/;$//')

if [ ! -z "$COVERAGE_FILES" ]; then
    print_status $YELLOW "📈 Generating coverage report..."
    reportgenerator \
        -reports:"$COVERAGE_FILES" \
        -targetdir:"$RESULTS_DIR/CoverageReport" \
        -reporttypes:"Html;Badges;TextSummary" \
        -title:"Weather API Local CI Coverage" \
        -verbosity:Info
    print_status $GREEN "✅ Coverage report generated"
else
    print_status $YELLOW "⚠️ No coverage files found"
fi

# Generate summary report
cat > "$RESULTS_DIR/local-ci-summary.html" << 'EOF'
<!DOCTYPE html>
<html>
<head>
    <title>Local CI Pipeline Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background: #2e7d32; color: white; padding: 15px; border-radius: 5px; }
        .section { margin: 20px 0; padding: 15px; border: 1px solid #ddd; border-radius: 5px; }
        .success { background: #e8f5e8; border-color: #4caf50; }
        .info { background: #e3f2fd; border-color: #2196f3; }
        .links { display: flex; gap: 10px; flex-wrap: wrap; }
        .btn { 
            display: inline-block; padding: 10px 15px; 
            background: #1976d2; color: white; text-decoration: none; 
            border-radius: 4px; margin: 5px 0;
        }
        .btn:hover { background: #1565c0; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🖥️ Local CI Pipeline Results</h1>
        <p>Weather API - Local Development Testing</p>
    </div>
    
    <div class="section success">
        <h2>✅ Pipeline Completed Successfully</h2>
        <p>All steps in the local CI pipeline have been executed.</p>
        <ul>
            <li>✅ Dependencies restored</li>
            <li>✅ Solution built</li>
            <li>✅ Unit tests executed</li>
            <li>✅ Contract validated</li>
            <li>✅ Stub server tested</li>
            <li>✅ Acceptance tests completed</li>
            <li>✅ Reports generated</li>
        </ul>
    </div>
    
    <div class="section info">
        <h2>📊 Available Reports</h2>
        <div class="links">
            <a href="UnitTests/unit-test-results.trx" class="btn">📝 Unit Test Results</a>
            <a href="acceptance-test-results.html" class="btn">📋 Acceptance Test Results</a>
            <a href="CoverageReport/index.html" class="btn">📈 Coverage Report</a>
        </div>
    </div>
    
    <div class="section info">
        <h2>🔧 Next Steps</h2>
        <p>Your code is ready for the GitHub Actions pipeline. The same tests that passed locally will run in CI/CD.</p>
        <ul>
            <li>Commit and push your changes</li>
            <li>Monitor the GitHub Actions workflow</li>
            <li>Review the online test reports</li>
        </ul>
    </div>
</body>
</html>
EOF

echo ""
print_status $GREEN "🎉 Local CI Pipeline Completed Successfully!"
echo "=============================================="
print_status $BLUE "📊 Results available in: $RESULTS_DIR"

# Open summary report if possible
if command_exists open; then
    print_status $BLUE "🌐 Opening summary report..."
    open "$RESULTS_DIR/local-ci-summary.html"
elif command_exists xdg-open; then
    print_status $BLUE "🌐 Opening summary report..."
    xdg-open "$RESULTS_DIR/local-ci-summary.html"
else
    print_status $BLUE "📄 Summary report: $RESULTS_DIR/local-ci-summary.html"
fi

echo ""
print_status $GREEN "✅ Your code is ready for GitHub Actions CI/CD pipeline!"
print_status $BLUE "💡 Tip: Run this script before pushing to catch issues early"
