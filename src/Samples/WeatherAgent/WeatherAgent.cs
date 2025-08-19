using A2A;
using Microsoft.Extensions.AI;
using WeatherAgent.Services;

namespace WeatherAgent;

public class WeatherAgent(
    IChatClient chatClient,
    IWeatherService weatherService,
    ILocationDetectionService locationDetectionService)
{
    private ITaskManager? _taskManager;
    
    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
        taskManager.OnTaskCreated = ProcessTaskAsync;
        taskManager.OnTaskUpdated = ProcessTaskAsync;
    }

    private async Task ProcessTaskAsync(AgentTask agentTaskParams, CancellationToken cancellationToken)
    {
        var lastMessage = agentTaskParams.History!.Last();
        var messageText = lastMessage.Parts.OfType<TextPart>().First().Text;

        var locationCheck = await locationDetectionService.CheckForLocationAsync(messageText, cancellationToken);

        if (!locationCheck.HasLocation)
        {
            await _taskManager!.UpdateStatusAsync(
                agentTaskParams.Id,
                TaskState.InputRequired,
                final: false,
                cancellationToken: cancellationToken);

            await _taskManager!.ReturnArtifactAsync(agentTaskParams.Id, new Artifact
            {
                Parts =
                [
                    new TextPart
                    {
                        Text =
                            "Please provide a location to get the weather information. For example: 'What's the weather in New York?' or 'How's the weather in London today?'"
                    }
                ]
            }, cancellationToken);

            return;
        }

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful weather assistant. Use the GetWeatherAsync function to provide weather information for the requested location."),
            new(ChatRole.User, messageText)
        };

        // Define the weather function using AIFunctionFactory
        var weatherFunction = AIFunctionFactory.Create(weatherService.GetWeatherAsync);

        // Configure chat options with function calling
        var chatOptions = new ChatOptions
        {
            Tools = [weatherFunction],
            ToolMode = ChatToolMode.Auto,
            Temperature = 0.7f,
            MaxOutputTokens = 500
        };

        // Get response from chat client with automatic function calling
        var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
        
        await _taskManager!.UpdateStatusAsync(
            agentTaskParams.Id,
            TaskState.Completed,
            final: true,
            cancellationToken: cancellationToken);
    
        await _taskManager!.ReturnArtifactAsync(agentTaskParams.Id, new Artifact
        {
            Parts = [new TextPart { Text = response.Text }]
        }, cancellationToken);
    }
    
    private async Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new AgentCard
        {
            Name = "Weather Agent",
            Description = "An intelligent weather agent powered by Microsoft.Extensions.AI that provides weather information for any location",
            Url = agentUrl,
            Version = "1.0.0",
            Capabilities = new AgentCapabilities { Streaming = true },
            Skills =
            [
                new AgentSkill
                {
                    Name = "Get Weather",
                    Description = "Provides current weather information for a specified location"
                }
            ]
        });
    }
}