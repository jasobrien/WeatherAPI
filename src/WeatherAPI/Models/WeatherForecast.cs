namespace WeatherAPI.Models;

public class WeatherForecast
{
    public int hour { get; set; }
    public int temperatureC { get; set; }
    public double rainfallMm { get; set; }

    public int temperatureF => 32 + (int)(temperatureC / 0.5556);
}
