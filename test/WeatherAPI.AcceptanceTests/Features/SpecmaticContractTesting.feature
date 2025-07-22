Feature: Weather API Contract Testing with Specmatic
    As a developer
    I want to verify that the Weather API meets its contract specifications
    So that consumers can depend on consistent API behavior

Background:
    Given the Specmatic stub server is running
    And the weather forecast contract is loaded

Scenario: Get weather forecast returns valid contract response
    When I request the weather forecast from Specmatic
    Then the response should have status code 200
    And the response should contain 24 hourly forecasts
    And each forecast should have required properties
    And the response should match the contract schema

Scenario: Get weather forecast with specific location
    Given I have a custom stub for location "London"
    When I request the weather forecast for "London"
    Then the response should have status code 200
    And the response should contain location-specific data

Scenario: Handle invalid endpoint requests
    When I request an invalid endpoint "/api/InvalidEndpoint"
    Then the response should have status code 404
    And the response should match error contract

Scenario: Contract validation for concurrent requests
    When I make 5 concurrent requests to the weather forecast endpoint
    Then all responses should have status code 200
    And all responses should match the contract schema
    And response times should be reasonable

Scenario: Dynamic stub creation and testing
    Given I create a custom stub for extreme weather conditions
    When I request the weather forecast from Specmatic
    Then the response should contain the extreme weather data
    And the response should still match the contract schema

Scenario Outline: Weather forecast error scenarios
    When I request the weather forecast with <condition>
    Then the response should have status code <status_code>
    And the error response should match the contract

    Examples:
    | condition                | status_code |
    | invalid query parameter  | 400         |
    | unauthorized access      | 401         |
    | service unavailable      | 503         |
