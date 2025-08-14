using WeatherAgent.Models;

namespace WeatherAgent.Services;

public interface ILocationDetectionService
{
    Task<LocationCheckResult> CheckForLocationAsync(string message, CancellationToken cancellationToken);
}