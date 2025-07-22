#!/bin/bash

# Weather API Acceptance Tests with HTML Report Generation
echo "🚀 Running Weather API Acceptance Tests with HTML Report..."

# Clean previous test results
echo "🧹 Cleaning previous test results..."
rm -rf TestResults/
mkdir -p TestResults/

# Run the tests with detailed logging and generate TRX + HTML output
echo "🧪 Running tests..."
dotnet test \
    --logger "trx;LogFileName=TestResults.trx" \
    --logger "html;LogFileName=TestResults.html" \
    --logger "console;verbosity=detailed" \
    --results-directory TestResults/ \
    --collect:"XPlat Code Coverage"

# Check if tests completed
TEST_EXIT_CODE=$?
if [ $TEST_EXIT_CODE -eq 0 ]; then
    echo "✅ Tests completed successfully!"
else
    echo "❌ Tests completed with failures (Exit code: $TEST_EXIT_CODE)"
fi

# Generate enhanced HTML report using ReportGenerator if available
echo "📊 Generating enhanced HTML report..."
if command -v dotnet &> /dev/null; then
    if dotnet tool list -g | grep -q reportgenerator; then
        echo "🔧 Using ReportGenerator for enhanced reporting..."
        dotnet reportgenerator \
            "-reports:TestResults/**/*.xml" \
            "-targetdir:TestResults/CoverageReport" \
            "-reporttypes:Html"
    else
        echo "⚠️  ReportGenerator not installed globally. Installing..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
        if [ $? -eq 0 ]; then
            dotnet reportgenerator \
                "-reports:TestResults/**/*.xml" \
                "-targetdir:TestResults/CoverageReport" \
                "-reporttypes:Html"
        fi
    fi
fi

# List generated reports
echo "📄 Generated reports:"
if [ -f "TestResults/TestResults.html" ]; then
    echo "  ✅ Basic HTML Report: TestResults/TestResults.html"
fi
if [ -f "TestResults/TestResults.trx" ]; then
    echo "  ✅ TRX Report: TestResults/TestResults.trx"
fi
if [ -d "TestResults/CoverageReport" ]; then
    echo "  ✅ Coverage Report: TestResults/CoverageReport/index.html"
fi

# Create a simple HTML report from test results
echo "📋 Creating custom test summary report..."
cat > TestResults/test-summary.html << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Weather API Contract Tests - Test Results</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 10px; }
        .summary { display: flex; gap: 20px; margin: 20px 0; }
        .card { background: #ecf0f1; padding: 15px; border-radius: 5px; flex: 1; text-align: center; }
        .passed { background: #d5e8d4; border-left: 4px solid #27ae60; }
        .failed { background: #f8d7da; border-left: 4px solid #e74c3c; }
        .info { background: #d1ecf1; border-left: 4px solid #17a2b8; }
        .features { margin-top: 20px; }
        .feature { margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 4px; }
        .timestamp { color: #666; font-size: 0.9em; }
        .links { margin-top: 20px; }
        .link-button { display: inline-block; padding: 10px 20px; margin: 5px; background: #3498db; color: white; text-decoration: none; border-radius: 4px; }
        .link-button:hover { background: #2980b9; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🌤️ Weather API Contract Tests</h1>
        <p class="timestamp">Report generated: $(date)</p>
        
        <div class="summary">
            <div class="card info">
                <h3>Test Run</h3>
                <p>Status: $([ $TEST_EXIT_CODE -eq 0 ] && echo "✅ Success" || echo "❌ Failed")</p>
            </div>
            <div class="card">
                <h3>Framework</h3>
                <p>Reqnroll + Specmatic</p>
            </div>
            <div class="card">
                <h3>Contract Testing</h3>
                <p>OpenAPI 3.0</p>
            </div>
        </div>

        <div class="features">
            <h2>📋 Test Features</h2>
            <div class="feature">
                <h3>✅ Specmatic Contract Testing</h3>
                <p>Comprehensive API contract validation using Specmatic test containers</p>
                <ul>
                    <li>Basic weather forecast retrieval</li>
                    <li>Location-specific weather data</li>
                    <li>Error scenarios (400, 401, 503)</li>
                    <li>Concurrent request handling</li>
                    <li>Dynamic stub creation</li>
                </ul>
            </div>
        </div>

        <div class="links">
            <h2>📊 Detailed Reports</h2>
EOF

# Add links to available reports
if [ -f "TestResults/TestResults.html" ]; then
    echo '            <a href="TestResults.html" class="link-button">📄 HTML Test Results</a>' >> TestResults/test-summary.html
fi
if [ -f "TestResults/TestResults.trx" ]; then
    echo '            <a href="TestResults.trx" class="link-button">📋 TRX Results</a>' >> TestResults/test-summary.html
fi
if [ -d "TestResults/CoverageReport" ]; then
    echo '            <a href="CoverageReport/index.html" class="link-button">📈 Coverage Report</a>' >> TestResults/test-summary.html
fi

cat >> TestResults/test-summary.html << 'EOF'
        </div>

        <div style="margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 0.9em;">
            <p>This report was automatically generated during test execution.</p>
        </div>
    </div>
</body>
</html>
EOF

echo "✅ Custom summary report created: TestResults/test-summary.html"

# Open the summary report in default browser (optional)
if command -v open &> /dev/null; then
    echo "🌐 Opening test summary in browser..."
    open TestResults/test-summary.html
fi

echo "🏁 Done! Test reports available in TestResults/ directory."
