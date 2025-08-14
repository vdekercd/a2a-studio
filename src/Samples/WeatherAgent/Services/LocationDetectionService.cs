using System.Text.Json;
using Microsoft.Extensions.AI;
using WeatherAgent.Models;
using ChatMessage = Microsoft.Extensions.AI;

namespace WeatherAgent.Services;

public class LocationDetectionService(IChatClient chatClient) : ILocationDetectionService
{
    private const string SystemPrompt = """
        You are a location detection assistant. Analyze the user's message to determine if it contains a specific location for a weather query.

        Respond with ONLY a JSON object in this exact format:
        {
            "hasLocation": true/false,
            "location": "extracted location or null"
        }

        Examples:
        - "What's the weather in Paris?" -> {"hasLocation": true, "location": "Paris"}
        - "How's the weather today?" -> {"hasLocation": false, "location": null}
        - "Is it raining in Tokyo?" -> {"hasLocation": true, "location": "Tokyo"}
        """;

    public async Task<LocationCheckResult> CheckForLocationAsync(string message, CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage.ChatMessage>
        {
            new(ChatRole.System, SystemPrompt),
            new(ChatRole.User, $"Analyze this message: \"{message}\"")
        };

        var chatOptions = new ChatOptions
        {
            Temperature = 0.1f,
            MaxOutputTokens = 150
        };

        try
        {
            var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
            return ParseLocationResponse(response.Text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in location detection: {ex.Message}");
            return new LocationCheckResult { HasLocation = false };
        }
    }

    private static LocationCheckResult ParseLocationResponse(string? jsonResponse)
    {
        if (string.IsNullOrEmpty(jsonResponse))
            return new LocationCheckResult { HasLocation = false };

        try
        {
            var cleanedResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();
            
            var result = JsonSerializer.Deserialize<LocationCheckResult>(cleanedResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new LocationCheckResult { HasLocation = false };
        }
        catch
        {
            return new LocationCheckResult { HasLocation = false };
        }
    }
}