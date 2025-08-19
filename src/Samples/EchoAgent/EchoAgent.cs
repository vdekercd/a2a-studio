using A2A;
using System.Text.Json;

namespace EchoAgent;

public class EchoAgent
{
    private ITaskManager? _taskManager;

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        //taskManager.OnMessageReceived = ProcessMessageAsync;
        taskManager.OnTaskCreated = ProcessTaskAsync;
        taskManager.OnTaskUpdated = ProcessTaskAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

    private Task<Message> ProcessMessageAsync(MessageSendParams messageSendParams, CancellationToken ct)
    {
        // Get incoming message text
        string request = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

        // Check if this is a task (starts with TASK:)
        string responsePrefix;
        string cleanRequest = request;
        List<Part> responseParts = new();
        
        if (request.StartsWith("TASK:", StringComparison.OrdinalIgnoreCase))
        {
            responsePrefix = "ECHO TASK COMPLETED:";
            cleanRequest = request.Substring(5).Trim(); // Remove "TASK:" prefix
            
            // For task-like messages, return enhanced artifacts like ProcessTaskAsync does
            var textPart = new TextPart() { Text = $"{responsePrefix} {cleanRequest}" };
            responseParts.Add(textPart);
            
            // Generate SVG content
            var svgContent = GenerateTaskResultSvg(cleanRequest, Guid.NewGuid().ToString("N")[..8]);
            var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
            
            // Add FilePart with SVG
            var filePart = new FilePart() {
                File = new FileWithBytes()
                {
                    Name = $"task_result.svg",
                    MimeType = "image/svg+xml",
                    Bytes = Convert.ToBase64String(svgBytes)
                }
            };
            responseParts.Add(filePart);
            
            // Generate sample JSON data
            var jsonData = GenerateTaskResultData(cleanRequest, Guid.NewGuid().ToString("N")[..8]);
            
            // Add DataPart with JSON
            var dataPart = new DataPart() {
                Data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData)!
            };
            responseParts.Add(dataPart);
        }
        else
        {
            responsePrefix = "ECHO MESSAGE:";
            responseParts.Add(new TextPart() { Text = $"{responsePrefix} {cleanRequest}" });
        }

        // Create and return the response message
        var response = new Message()
        {
            Role = MessageRole.Agent,
            MessageId = Guid.NewGuid().ToString(),
            ContextId = messageSendParams.Message.ContextId,
            Parts = responseParts
        };

        return Task.FromResult(response);
    }

    private async Task ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Process the task
        var lastMessage = task.History!.Last();
        var messageText = lastMessage.Parts.OfType<TextPart>().First().Text;

        // Check for target-state metadata to determine task behavior
        TaskState targetState = GetTargetStateFromMetadata(lastMessage.Metadata) ?? TaskState.Completed;

        // Generate SVG content
        var svgContent = GenerateTaskResultSvg(messageText, task.Id);
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);

        // Generate sample JSON data
        var jsonData = GenerateTaskResultData(messageText, task.Id);
        var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Return task artifact with multiple parts: TextPart, FilePart, and DataPart
        await _taskManager!.ReturnArtifactAsync(task.Id, new Artifact()
        {
            Parts = [
                new TextPart() { 
                    Text = $"ECHO TASK COMPLETED: {messageText}" 
                },
                new FilePart() {
                    File = new FileWithUri()
                    {
                        Name = $"https://s.w.org/images/core/emoji/16.0.1/svg/1f30a.svg",
                        MimeType = "image/svg+xml",
                    }
                },
                new DataPart() {
                    Data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData)!
                }
            ]
        }, cancellationToken);

        // Update task status to completed
        await _taskManager!.UpdateStatusAsync(
            task.Id,
            status: targetState,
            final: targetState is TaskState.Completed or TaskState.Canceled or TaskState.Failed or TaskState.Rejected,
            cancellationToken: cancellationToken);
    }

    private static TaskState? GetTargetStateFromMetadata(Dictionary<string, JsonElement>? metadata)
    {
        if (metadata?.TryGetValue("task-target-state", out var targetStateElement) == true)
        {
            if (Enum.TryParse<TaskState>(targetStateElement.GetString(), true, out var state))
            {
                return state;
            }
        }
        return null;
    }

    private static string GenerateTaskResultSvg(string messageText, string taskId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var messageLength = messageText.Length;
        
        return $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <svg width="400" height="200" xmlns="http://www.w3.org/2000/svg">
                <defs>
                    <linearGradient id="grad" x1="0%" y1="0%" x2="100%" y2="100%">
                        <stop offset="0%" style="stop-color:#4fc3f7;stop-opacity:1" />
                        <stop offset="100%" style="stop-color:#29b6f6;stop-opacity:1" />
                    </linearGradient>
                </defs>
                <rect width="400" height="200" fill="url(#grad)" rx="10" ry="10"/>
                <rect x="10" y="10" width="380" height="180" fill="rgba(255,255,255,0.1)" rx="8" ry="8" stroke="rgba(255,255,255,0.3)" stroke-width="1"/>
                
                <text x="20" y="40" font-family="Arial, sans-serif" font-size="18" font-weight="bold" fill="white">
                    Task Result Visualization
                </text>
                
                <text x="20" y="70" font-family="Arial, sans-serif" font-size="12" fill="rgba(255,255,255,0.9)">
                    Task ID: {taskId}
                </text>
                
                <text x="20" y="90" font-family="Arial, sans-serif" font-size="12" fill="rgba(255,255,255,0.9)">
                    Generated: {timestamp}
                </text>
                
                <text x="20" y="110" font-family="Arial, sans-serif" font-size="12" fill="rgba(255,255,255,0.9)">
                    Message Length: {messageLength} characters
                </text>
                
                <rect x="20" y="130" width="{Math.Min(360, messageLength * 5)}" height="15" fill="rgba(255,255,255,0.3)" rx="7" ry="7"/>
                <rect x="20" y="130" width="{Math.Min(360, messageLength * 5)}" height="15" fill="rgba(255,255,255,0.8)" rx="7" ry="7"/>
                
                <text x="20" y="170" font-family="Arial, sans-serif" font-size="10" fill="rgba(255,255,255,0.7)">
                    Echo Agent - Task Processing Complete
                </text>
            </svg>
            """;
    }

    private static string GenerateTaskResultData(string messageText, string taskId)
    {
        var data = new
        {
            taskId = taskId,
            status = "completed",
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            input = new
            {
                message = messageText,
                length = messageText.Length,
                wordCount = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
            },
            processing = new
            {
                duration_ms = new Random().Next(100, 1000),
                agent = "EchoAgent",
                version = "1.1.0"
            },
            output = new
            {
                echo = $"ECHO TASK COMPLETED: {messageText}",
                metadata = new
                {
                    processed_at = DateTime.UtcNow,
                    character_analysis = new
                    {
                        uppercase_count = messageText.Count(char.IsUpper),
                        lowercase_count = messageText.Count(char.IsLower),
                        digit_count = messageText.Count(char.IsDigit),
                        space_count = messageText.Count(char.IsWhiteSpace)
                    }
                }
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        return await Task.FromResult<AgentCard>(new AgentCard()
        {
            Name = "Echo Agent Demo",
            Description = "A simple demonstration agent that echoes back any message it receives",
            Url = "http://localhost:5000/",
            Capabilities = new AgentCapabilities() { Streaming = true },
            Skills =
            [
                new AgentSkill
                {
                    Name = "Echo",
                    Description = "Echoes back the received message"
                }
            ]
        });
    }
}