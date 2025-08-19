using A2AStudio.Models;

namespace A2AStudio.Services;

public class A2AService(ILogger<A2AService> logger) : IA2AService
{
    private AgentCard? _agentCard;
    private AgentCardInfo? _connectedAgentInfo;
    private A2AClient? _cachedClient;
    
    public bool IsConnected => _connectedAgentInfo != null && _agentCard != null && _cachedClient != null;
    public AgentCardInfo? ConnectedAgent => _connectedAgentInfo;
    
    public string? GetAgentCardJson()
    {
        if (_agentCard == null)
            return null;
            
        try
        {
            return JsonSerializer.Serialize(_agentCard, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to serialize agent card to JSON");
            return null;
        }
    }
    
    public void Disconnect()
    {
        _agentCard = null;
        _connectedAgentInfo = null;
        _cachedClient = null;
    }
    public async Task<ConnectionResult> ConnectToAgentAsync(string agentUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(agentUrl))
            {
                return new ConnectionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Agent URL cannot be empty"
                };
            }

            if (!Uri.TryCreate(agentUrl, UriKind.Absolute, out var uri))
            {
                return new ConnectionResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid URL format"
                };
            }

            var cardResolver = new A2ACardResolver(uri);
            _agentCard = await cardResolver.GetAgentCardAsync();

            // Validate the agent card
            var validationResult = AgentCardValidator.ValidateAgentCard(_agentCard);
            
            _connectedAgentInfo = new AgentCardInfo
            {
                Name = _agentCard.Name,
                Description = _agentCard.Description,
                Version = _agentCard.Version,
                Url = _agentCard.Url,
                ImageUrl = null,
                Capabilities = ExtractCapabilities(_agentCard.Capabilities),
                Tags = [],
                LastUpdated = DateTime.UtcNow,
                IsConnected = true,
                ValidationResult = validationResult
            };
            
            _cachedClient = new A2AClient(new Uri(_agentCard.Url));

            return new ConnectionResult
            {
                IsSuccess = true,
                AgentCard = _connectedAgentInfo
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to agent at {AgentUrl}", agentUrl);
            return new ConnectionResult
            {
                IsSuccess = false,
                ErrorMessage = $"Connection failed: {ex.Message}"
            };
        }
    }
    
    public async Task<MessageResult> SendMessageAsync(string message, string? taskId, IEnumerable<string>? previousMessages)
    {
        try
        {
            if (!IsConnected || _cachedClient == null)
            {
                return new MessageResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Not connected to any agent"
                };
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return new MessageResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Message cannot be empty"
                };
            }
            
            var messageParams = new MessageSendParams
            {
                Message = new Message
                {
                    Role = MessageRole.User,
                    Parts = [new TextPart { Text = message }]
                }
            };
            
            if (!string.IsNullOrWhiteSpace(taskId))
            {
                messageParams.Message.TaskId = taskId;
            }
            
            var response = await _cachedClient.SendMessageAsync(messageParams);
            
            logger.LogInformation("Received response of type: {ResponseType}", response.GetType().Name);
            
            if (response is Message messageResponse)
            {
                return ProcessParts(messageResponse.Parts, previousMessages);
            }
            
            if (response is AgentTask agentTaskResponse)
            {
                var allParts = new List<Part>();
                
                if (agentTaskResponse.History != null && agentTaskResponse.History.Any())
                {
                    var latestAgentMessage = agentTaskResponse.History
                        .Where(m => m.Role != MessageRole.User)
                        .OrderByDescending(m => m.Parts?.Count ?? 0)
                        .FirstOrDefault();
                        
                    if (latestAgentMessage?.Parts != null)
                    {
                        allParts.AddRange(latestAgentMessage.Parts);
                    }
                }
                
                if (agentTaskResponse.Artifacts != null)
                {
                    foreach (var artifact in agentTaskResponse.Artifacts)
                    {
                        allParts.AddRange(artifact.Parts);
                    }
                }
                
                var result = ProcessParts(allParts, previousMessages);
                result.IsTask = true;
                result.TaskId = agentTaskResponse.Id ?? Guid.NewGuid().ToString("N")[..8];
                result.TaskStatus = ConvertAgentTaskStatusToString(agentTaskResponse.Status);
                return result;
            }
            
            return new MessageResult
            {
                IsSuccess = false,
                ErrorMessage = $"Unknown response type: {response.GetType().Name}",
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to agent");
            return new MessageResult
            {
                IsSuccess = false,
                ErrorMessage = $"Message sending failed: {ex.Message}"
            };
        }
    }

    private MessageResult ProcessParts(IEnumerable<Part>? parts, IEnumerable<string>? previousMessages = null)
    {
        var result = new MessageResult { IsSuccess = true };
        var textParts = new List<string>();

        if (parts != null)
        {
            foreach (var part in parts)
            {
                switch (part)
                {
                    case TextPart textPart when !string.IsNullOrEmpty(textPart.Text):
                        // Apply deduplication to text parts
                        var deduplicatedText = RemoveDuplicateContent(textPart.Text, previousMessages);
                        if (!string.IsNullOrWhiteSpace(deduplicatedText))
                        {
                            textParts.Add(deduplicatedText);
                            result.Parts.Add(new PartResult
                            {
                                Type = "text",
                                Text = deduplicatedText
                            });
                        }
                        break;

                    case FilePart filePart when filePart.File is FileWithBytes fileBytes:
                        result.Parts.Add(new PartResult
                        {
                            Type = "file",
                            FileName = fileBytes.Name,
                            MimeType = fileBytes.MimeType,
                            FileBytes = Convert.FromBase64String(fileBytes.Bytes)
                        });
                        break;

                    case FilePart filePart when filePart.File is FileWithUri fileUri:
                        result.Parts.Add(new PartResult
                        {
                            Type = "file",
                            FileName = fileUri.Name,
                            MimeType = fileUri.MimeType,
                            FileUri = fileUri.Uri
                        });
                        break;

                    case DataPart dataPart:
                        var jsonData = JsonSerializer.Serialize(dataPart.Data, new JsonSerializerOptions { WriteIndented = true });
                        result.Parts.Add(new PartResult
                        {
                            Type = "data",
                            JsonData = jsonData
                        });
                        break;
                }
            }
        }

        result.Response = textParts.Any() ? string.Join(" ", textParts) : "Response received";
        
        // Simple detection: if response has FilePart or DataPart, it's a task
        var hasEnhancedParts = result.Parts.Any(p => p.Type == "file" || p.Type == "data");
        if (hasEnhancedParts)
        {
            result.IsTask = true;
            result.TaskId = Guid.NewGuid().ToString("N")[..8];
            result.TaskStatus = "Completed";
        }
        
        return result;
    }

    private List<string> ExtractCapabilities(AgentCapabilities? capabilities)
    {
        var result = new List<string>();
        
        if (capabilities == null)
            return result;
        
        if (capabilities.Streaming)
            result.Add("Streaming");
        
        return result;
    }

    private string ConvertAgentTaskStatusToString(AgentTaskStatus status)
    {
        // AgentTaskStatus has a state property that contains the actual status string
        var statusString = status.State.ToString() ?? "Unknown";
        
        // Handle the 9 specified TaskState values
        return statusString switch
        {
            "Submitted" => "Submitted",
            "Working" => "Working",
            "InputRequired" => "Input Required",
            "Completed" => "Completed",
            "Canceled" => "Cancelled",
            "Failed" => "Failed",
            "Rejected" => "Rejected",
            "AuthRequired" => "Auth Required",
            "Unknown" => "Unknown",
            _ => statusString // Return as-is for any other values
        };
    }

    private string RemoveDuplicateContent(string responseText, IEnumerable<string>? previousMessages)
    {
        if (string.IsNullOrWhiteSpace(responseText) || previousMessages == null || !previousMessages.Any())
        {
            return responseText;
        }

        var cleanedText = responseText;
        var previousMessagesList = previousMessages.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();

        // Remove exact matches of previous messages
        foreach (var prevMessage in previousMessagesList)
        {
            // Remove exact duplicates (case-insensitive)
            cleanedText = cleanedText.Replace(prevMessage, "", StringComparison.OrdinalIgnoreCase);
        }

        // Remove common conversation patterns that indicate duplication
        var patterns = new[]
        {
            @"User:\s*.+?(?=Agent:|Assistant:|$)", // Remove "User: ..." patterns
            @"Agent:\s*.+?(?=User:|$)", // Remove "Agent: ..." patterns  
            @"Assistant:\s*.+?(?=User:|$)", // Remove "Assistant: ..." patterns
            @"Human:\s*.+?(?=Assistant:|$)", // Remove "Human: ..." patterns
            @"\*\*User\*\*:\s*.+?(?=\*\*Agent\*\*:|\*\*Assistant\*\*:|$)", // Remove **User**: ... patterns
            @"\*\*Agent\*\*:\s*.+?(?=\*\*User\*\*:|$)", // Remove **Agent**: ... patterns
            @"\*\*Assistant\*\*:\s*.+?(?=\*\*User\*\*:|$)", // Remove **Assistant**: ... patterns
        };

        foreach (var pattern in patterns)
        {
            cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, pattern, "", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                System.Text.RegularExpressions.RegexOptions.Multiline);
        }

        // Clean up extra whitespace and newlines
        cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"\s{2,}", " ");
        cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"\n{2,}", "\n");
        
        return cleanedText.Trim();
    }
}