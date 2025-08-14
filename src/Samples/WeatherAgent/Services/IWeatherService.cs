using System.ComponentModel;

namespace WeatherAgent.Services;

public interface IWeatherService
{
    [Description("Get the current weather for a specified location")]
    Task<string> GetWeatherAsync([Description("The city or location to get weather for")] string location);
}