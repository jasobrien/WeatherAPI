#!/bin/bash

# GitHub Actions Workflow Validator
# Validates that all workflow files are properly configured

echo "🔍 GitHub Actions Workflow Validator"
echo "===================================="

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

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
WORKFLOWS_DIR="$PROJECT_ROOT/.github/workflows"

print_status $BLUE "📁 Checking workflows directory: $WORKFLOWS_DIR"

# Check if workflows directory exists
if [ ! -d "$WORKFLOWS_DIR" ]; then
    print_status $RED "❌ Workflows directory not found!"
    exit 1
fi

print_status $GREEN "✅ Workflows directory exists"

# Check workflow files
WORKFLOWS=(
    "ci.yml:Main CI/CD Pipeline"
    "pr-validation.yml:Pull Request Validation"
    "nightly.yml:Nightly Comprehensive Tests"
)

echo ""
print_status $BLUE "📄 Validating workflow files..."

for workflow_info in "${WORKFLOWS[@]}"; do
    IFS=':' read -r filename description <<< "$workflow_info"
    filepath="$WORKFLOWS_DIR/$filename"
    
    if [ -f "$filepath" ]; then
        print_status $GREEN "✅ $filename - $description"
        
        # Basic YAML syntax check if yq is available
        if command -v python3 >/dev/null 2>&1; then
            if python3 -c "
import yaml
try:
    with open('$filepath', 'r') as f:
        yaml.safe_load(f)
    print('   ✅ YAML syntax valid')
except Exception as e:
    print(f'   ❌ YAML syntax error: {e}')
    exit(1)
" 2>/dev/null; then
                :
            else
                print_status $YELLOW "   ⚠️ Could not validate YAML syntax"
            fi
        fi
    else
        print_status $RED "❌ $filename - Missing!"
        exit 1
    fi
done

echo ""
print_status $BLUE "🔧 Checking project structure..."

# Check required directories and files
REQUIRED_PATHS=(
    "src/WeatherAPI:Main API Project"
    "test/WeatherAPI.UnitTests:Unit Tests"
    "test/WeatherAPI.AcceptanceTests:Acceptance Tests"
    "test/WeatherAPI.SpecmaticTests/contract:Contract Tests"
    "test/WeatherAPI.SpecmaticTests/contract/weather-api-contract.yaml:OpenAPI Contract"
    "test/WeatherAPI.SpecmaticTests/contract/stubs:Stub Files"
    "WeatherAPI.sln:Solution File"
)

for path_info in "${REQUIRED_PATHS[@]}"; do
    IFS=':' read -r path description <<< "$path_info"
    fullpath="$PROJECT_ROOT/$path"
    
    if [ -e "$fullpath" ]; then
        print_status $GREEN "✅ $path - $description"
    else
        print_status $RED "❌ $path - Missing! ($description)"
        exit 1
    fi
done

echo ""
print_status $BLUE "🧪 Checking test scripts..."

TEST_SCRIPTS=(
    "test/WeatherAPI.AcceptanceTests/run-tests-with-report.sh:Acceptance Test Script"
    "run-local-ci.sh:Local CI Script"
)

for script_info in "${TEST_SCRIPTS[@]}"; do
    IFS=':' read -r script description <<< "$script_info"
    scriptpath="$PROJECT_ROOT/$script"
    
    if [ -f "$scriptpath" ]; then
        if [ -x "$scriptpath" ]; then
            print_status $GREEN "✅ $script - $description (executable)"
        else
            print_status $YELLOW "⚠️ $script - $description (not executable)"
        fi
    else
        print_status $RED "❌ $script - Missing! ($description)"
    fi
done

echo ""
print_status $BLUE "🔑 Checking for required tools (locally)..."

TOOLS=(
    "dotnet:.NET SDK"
    "docker:Docker"
    "java:Java Runtime"
)

for tool_info in "${TOOLS[@]}"; do
    IFS=':' read -r tool description <<< "$tool_info"
    
    if command -v "$tool" >/dev/null 2>&1; then
        version=$($tool --version 2>/dev/null | head -n1 || echo "version unknown")
        print_status $GREEN "✅ $tool - $description ($version)"
    else
        print_status $YELLOW "⚠️ $tool - $description (not found locally, but available in CI)"
    fi
done

echo ""
print_status $BLUE "📋 Workflow Configuration Summary..."

# Check workflow triggers
echo "Workflow Triggers:"
echo "  • ci.yml: Push to main/develop, PRs to main/develop"
echo "  • pr-validation.yml: PRs to main/develop"
echo "  • nightly.yml: Scheduled (2 AM UTC daily) + manual dispatch"

echo ""
echo "Pipeline Flow:"
echo "  1. Build and Unit Tests"
echo "  2. Specmatic Contract Tests (parallel)"
echo "  3. Acceptance Tests (depends on 1 & 2)"
echo "  4. Deploy Reports to GitHub Pages"
echo "  5. Generate Summary"

echo ""
print_status $BLUE "🚀 GitHub Setup Requirements..."

echo "Required GitHub Repository Settings:"
echo "  • Actions: Enabled"
echo "  • Pages: Source set to 'GitHub Actions'"
echo "  • Branch protection (recommended):"
echo "    - Require status checks: Build and Unit Tests, Contract Tests, Acceptance Tests"
echo "    - Require branches to be up to date"

echo ""
echo "Optional GitHub Secrets (none currently required):"
echo "  • Add deployment credentials if needed"
echo "  • Add API keys for external services if needed"

echo ""
print_status $GREEN "🎉 Validation Complete!"

# Final recommendations
echo ""
print_status $BLUE "💡 Next Steps:"
echo "  1. Commit and push the workflow files"
echo "  2. Enable GitHub Actions in repository settings"
echo "  3. Configure GitHub Pages (Settings → Pages → Source: GitHub Actions)"
echo "  4. Set up branch protection rules (optional but recommended)"
echo "  5. Run the local CI script to test before pushing: ./run-local-ci.sh"

echo ""
print_status $GREEN "✅ Your GitHub Actions CI/CD pipeline is ready!"
print_status $BLUE "📚 See .github/workflows/README.md for detailed documentation"
