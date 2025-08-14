using System.ComponentModel;
using WeatherAgent.Models;

namespace WeatherAgent.Services;

public class WeatherService : IWeatherService
{
    [Description("Get the current weather for a specified location")]
    public async Task<string> GetWeatherAsync([Description("The city or location to get weather for")] string location)
    {
        // Simulate API call delay
        await Task.Delay(500);

        var weatherData = GenerateMockWeatherData(location);
        return FormatWeatherResponse(weatherData);
    }

    private static WeatherData GenerateMockWeatherData(string location)
    {
        var random = new Random();
        var conditions = new[] { "Sunny", "Partly Cloudy", "Cloudy", "Rainy", "Stormy" };

        return new WeatherData
        {
            Location = location,
            Temperature = random.Next(0, 35),
            Condition = conditions[random.Next(conditions.Length)],
            Humidity = random.Next(30, 90),
            WindSpeed = random.Next(5, 30),
            Unit = "Celsius",
            LastUpdated = DateTime.Now
        };
    }

    private static string FormatWeatherResponse(WeatherData weatherData)
    {
        return $"""
            Weather for {weatherData.Location}:
            🌡️ Temperature: {weatherData.Temperature}°{weatherData.Unit}
            ☁️ Condition: {weatherData.Condition}
            💧 Humidity: {weatherData.Humidity}%
            💨 Wind Speed: {weatherData.WindSpeed} km/h

            Last updated: {weatherData.LastUpdated:yyyy-MM-dd HH:mm:ss}
        """;
    }
}