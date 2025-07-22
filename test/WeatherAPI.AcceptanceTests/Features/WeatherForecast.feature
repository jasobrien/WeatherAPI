Feature: Weather Forecast API
    As a client application
    I want to retrieve weather forecasts
    So that I can display weather information to users

Scenario: Get 24-hour weather forecast
    When I request the weather forecast
    Then I should receive 24 hourly forecasts
    And each forecast should contain hour number, temperature, and rainfall data
    And the temperature should be between -5 and 34 degrees Celsius
    And the rainfall should be non-negative

Scenario: API returns proper HTTP status
    When I make a GET request to "/api/WeatherForecast"
    Then the response status code should be 200
    And the response should be in JSON format

Scenario: Weather forecast data validation
    When I request the weather forecast
    Then the hours should be numbered from 1 to 24
    And all rainfall values should be between 0 and 10 millimeters
