# GitHub Actions CI/CD - Quick Reference

## 🚀 Quick Start

1. **Push changes** to trigger the pipeline
2. **Monitor progress** in GitHub Actions tab
3. **View reports** at GitHub Pages URL
4. **Test locally** with `./run-local-ci.sh`

## 📊 Pipeline Status

| Workflow          | Status                          | Purpose                |
| ----------------- | ------------------------------- | ---------------------- |
| **Main CI/CD**    | Runs on push/PR to main/develop | Full testing + reports |
| **PR Validation** | Runs on all PRs                 | Quick validation       |
| **Nightly Tests** | Runs daily at 2 AM UTC          | Comprehensive testing  |

## 🔧 Available Scripts

```bash
# Run complete local testing
./run-local-ci.sh

# Validate GitHub Actions setup
./.github/workflows/validate-setup.sh

# Run acceptance tests with reports
cd test/WeatherAPI.AcceptanceTests && ./run-tests-with-report.sh
```

## 📋 Test Types

- **Unit Tests** → Fast feedback on code changes
- **Contract Tests** → OpenAPI compliance with Specmatic
- **Acceptance Tests** → End-to-end behavior validation
- **Integration Tests** → Full system workflow testing

## 🎯 Key Commands

```bash
# Build and test locally
dotnet restore
dotnet build --configuration Release
dotnet test

# Contract validation
docker run --rm -v "$(pwd):/app" -w /app znsio/specmatic:latest validate weather-api-contract.yaml

# Generate reports
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"coverage.xml" -targetdir:"reports"
```

## 🔍 Troubleshooting

| Issue                   | Solution                            |
| ----------------------- | ----------------------------------- |
| Build fails             | Check .NET version and dependencies |
| Container issues        | Ensure Docker is running            |
| Report generation fails | Install ReportGenerator tool        |
| Tests timeout           | Check Specmatic container startup   |

## 📈 Report Locations

- **GitHub Pages**: `https://yourusername.github.io/WeatherAPI/`
- **Workflow Artifacts**: Download from Actions → Workflow Run
- **Local Reports**: `LocalTestResults/` after running local CI

## 🔔 Notifications

- **✅ Success**: All tests pass, reports generated
- **❌ Failure**: Check workflow logs and artifacts
- **⚠️ Warning**: Some tests may have warnings but passed

---

**Need Help?** Check `GITHUB_ACTIONS_SETUP.md` for detailed documentation.
