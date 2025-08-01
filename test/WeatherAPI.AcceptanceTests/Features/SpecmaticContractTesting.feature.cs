﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by Reqnroll (https://www.reqnroll.net/).
//      Reqnroll Version:2.0.0.0
//      Reqnroll Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
using Reqnroll;
namespace WeatherAPI.AcceptanceTests.Features
{
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class WeatherAPIContractTestingWithSpecmaticFeature : object, Xunit.IClassFixture<WeatherAPIContractTestingWithSpecmaticFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private global::Reqnroll.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private static global::Reqnroll.FeatureInfo featureInfo = new global::Reqnroll.FeatureInfo(new global::System.Globalization.CultureInfo("en-US"), "Features", "Weather API Contract Testing with Specmatic", "    As a developer\n    I want to verify that the Weather API meets its contract s" +
                "pecifications\n    So that consumers can depend on consistent API behavior", global::Reqnroll.ProgrammingLanguage.CSharp, featureTags);
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "SpecmaticContractTesting.feature"
#line hidden
        
        public WeatherAPIContractTestingWithSpecmaticFeature(WeatherAPIContractTestingWithSpecmaticFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async global::System.Threading.Tasks.Task FeatureSetupAsync()
        {
        }
        
        public static async global::System.Threading.Tasks.Task FeatureTearDownAsync()
        {
        }
        
        public async global::System.Threading.Tasks.Task TestInitializeAsync()
        {
            testRunner = global::Reqnroll.TestRunnerManager.GetTestRunnerForAssembly(featureHint: featureInfo);
            try
            {
                if (((testRunner.FeatureContext != null) 
                            && (testRunner.FeatureContext.FeatureInfo.Equals(featureInfo) == false)))
                {
                    await testRunner.OnFeatureEndAsync();
                }
            }
            finally
            {
                if (((testRunner.FeatureContext != null) 
                            && testRunner.FeatureContext.BeforeFeatureHookFailed))
                {
                    throw new global::Reqnroll.ReqnrollException("Scenario skipped because of previous before feature hook error");
                }
                if ((testRunner.FeatureContext == null))
                {
                    await testRunner.OnFeatureStartAsync(featureInfo);
                }
            }
        }
        
        public async global::System.Threading.Tasks.Task TestTearDownAsync()
        {
            if ((testRunner == null))
            {
                return;
            }
            try
            {
                await testRunner.OnScenarioEndAsync();
            }
            finally
            {
                global::Reqnroll.TestRunnerManager.ReleaseTestRunner(testRunner);
                testRunner = null;
            }
        }
        
        public void ScenarioInitialize(global::Reqnroll.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public async global::System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async global::System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        public virtual async global::System.Threading.Tasks.Task FeatureBackgroundAsync()
        {
#line 6
#line hidden
#line 7
    await testRunner.GivenAsync("the Specmatic stub server is running", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 8
    await testRunner.AndAsync("the weather forecast contract is loaded", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
        }
        
        async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            try
            {
                await this.TestInitializeAsync();
            }
            catch (System.Exception e1)
            {
                try
                {
                    ((Xunit.IAsyncLifetime)(this)).DisposeAsync();
                }
                catch (System.Exception e2)
                {
                    throw new System.AggregateException("Test initialization failed", e1, e2);
                }
                throw;
            }
        }
        
        async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get weather forecast returns valid contract response")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Get weather forecast returns valid contract response")]
        public async global::System.Threading.Tasks.Task GetWeatherForecastReturnsValidContractResponse()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Get weather forecast returns valid contract response", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 10
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 11
    await testRunner.WhenAsync("I request the weather forecast from Specmatic", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 12
    await testRunner.ThenAsync("the response should have status code 200", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 13
    await testRunner.AndAsync("the response should contain 24 hourly forecasts", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 14
    await testRunner.AndAsync("each forecast should have required properties", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 15
    await testRunner.AndAsync("the response should match the contract schema", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get weather forecast with specific location")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Get weather forecast with specific location")]
        public async global::System.Threading.Tasks.Task GetWeatherForecastWithSpecificLocation()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Get weather forecast with specific location", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 17
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 18
    await testRunner.GivenAsync("I have a custom stub for location \"London\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 19
    await testRunner.WhenAsync("I request the weather forecast for \"London\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 20
    await testRunner.ThenAsync("the response should have status code 200", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 21
    await testRunner.AndAsync("the response should contain location-specific data", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Handle invalid endpoint requests")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Handle invalid endpoint requests")]
        public async global::System.Threading.Tasks.Task HandleInvalidEndpointRequests()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Handle invalid endpoint requests", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 23
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 24
    await testRunner.WhenAsync("I request an invalid endpoint \"/api/InvalidEndpoint\"", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 25
    await testRunner.ThenAsync("the response should have status code 404", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 26
    await testRunner.AndAsync("the response should match error contract", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Contract validation for concurrent requests")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Contract validation for concurrent requests")]
        public async global::System.Threading.Tasks.Task ContractValidationForConcurrentRequests()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Contract validation for concurrent requests", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 28
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 29
    await testRunner.WhenAsync("I make 5 concurrent requests to the weather forecast endpoint", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 30
    await testRunner.ThenAsync("all responses should have status code 200", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 31
    await testRunner.AndAsync("all responses should match the contract schema", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
#line 32
    await testRunner.AndAsync("response times should be reasonable", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Dynamic stub creation and testing")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Dynamic stub creation and testing")]
        public async global::System.Threading.Tasks.Task DynamicStubCreationAndTesting()
        {
            string[] tagsOfScenario = ((string[])(null));
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Dynamic stub creation and testing", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 34
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 35
    await testRunner.GivenAsync("I create a custom stub for extreme weather conditions", ((string)(null)), ((global::Reqnroll.Table)(null)), "Given ");
#line hidden
#line 36
    await testRunner.WhenAsync("I request the weather forecast from Specmatic", ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 37
    await testRunner.ThenAsync("the response should contain the extreme weather data", ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 38
    await testRunner.AndAsync("the response should still match the contract schema", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Weather forecast error scenarios")]
        [Xunit.TraitAttribute("FeatureTitle", "Weather API Contract Testing with Specmatic")]
        [Xunit.TraitAttribute("Description", "Weather forecast error scenarios")]
        [Xunit.InlineDataAttribute("invalid query parameter", "400", new string[0])]
        [Xunit.InlineDataAttribute("unauthorized access", "401", new string[0])]
        [Xunit.InlineDataAttribute("service unavailable", "503", new string[0])]
        public async global::System.Threading.Tasks.Task WeatherForecastErrorScenarios(string condition, string status_Code, string[] exampleTags)
        {
            string[] tagsOfScenario = exampleTags;
            global::System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new global::System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("condition", condition);
            argumentsOfScenario.Add("status_code", status_Code);
            global::Reqnroll.ScenarioInfo scenarioInfo = new global::Reqnroll.ScenarioInfo("Weather forecast error scenarios", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 40
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((global::Reqnroll.TagHelper.ContainsIgnoreTag(scenarioInfo.CombinedTags) || global::Reqnroll.TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
#line 6
await this.FeatureBackgroundAsync();
#line hidden
#line 41
    await testRunner.WhenAsync(string.Format("I request the weather forecast with {0}", condition), ((string)(null)), ((global::Reqnroll.Table)(null)), "When ");
#line hidden
#line 42
    await testRunner.ThenAsync(string.Format("the response should have status code {0}", status_Code), ((string)(null)), ((global::Reqnroll.Table)(null)), "Then ");
#line hidden
#line 43
    await testRunner.AndAsync("the error response should match the contract", ((string)(null)), ((global::Reqnroll.Table)(null)), "And ");
#line hidden
            }
            await this.ScenarioCleanupAsync();
        }
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Reqnroll", "2.0.0.0")]
        [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await WeatherAPIContractTestingWithSpecmaticFeature.FeatureSetupAsync();
            }
            
            async global::System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await WeatherAPIContractTestingWithSpecmaticFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
