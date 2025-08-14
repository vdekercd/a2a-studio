namespace A2AStudio.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] public IA2AService A2AService { get; set; } = null!;

    private string _agentUrl = string.Empty;
    private bool _isConnecting = false;
    private bool _isSendingMessage = false;
    
    private string? _errorMessage;
    
    private string _messageText = string.Empty;
    
    private readonly List<ChatMessage> _messageHistory = [];
    private string _selectedTaskId = string.Empty;
    
    // Theme management
    private bool _isDarkMode = true; // Default to dark mode


    private async Task ConnectToAgent()
    {
        if (string.IsNullOrWhiteSpace(_agentUrl))
            return;

        _isConnecting = true;
        _errorMessage = null;
        
        A2AService.Disconnect();

        try
        {
            var result = await A2AService.ConnectToAgentAsync(_agentUrl);
            
            if (result.IsSuccess)
            {
                _errorMessage = null;
                OnAgentConnected();
            }
            else
            {
                _errorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_messageText) || !A2AService.IsConnected)
            return;

        _isSendingMessage = true;
        var userMessage = _messageText;
        // Task detection will be handled by agent response
        _messageText = string.Empty; // Clear input immediately

        try
        {
            
            var targetTaskId = string.IsNullOrEmpty(_selectedTaskId) ? null : _selectedTaskId;
            var isExistingTask = !string.IsNullOrEmpty(targetTaskId);
            
            _messageHistory.Add(new ChatMessage
            {
                Text = userMessage,
                IsFromUser = true,
                Timestamp = DateTime.UtcNow,
                IsTask = isExistingTask,
                TaskId = targetTaskId,
                TaskStatus = isExistingTask ? "In Progress" : "Unknown"
            });

            var previousMessages = _messageHistory
                .Where(m => !m.IsFromUser && !string.IsNullOrWhiteSpace(m.Text))
                .Select(m => m.Text)
                .ToList();
            
            var result = await A2AService.SendMessageAsync(userMessage, targetTaskId, previousMessages);
            
            if (result.IsSuccess && !string.IsNullOrEmpty(result.Response))
            {
                var effectiveTaskId = result.TaskId ?? targetTaskId;
                bool isTask = !string.IsNullOrEmpty(effectiveTaskId);
                
                if (isTask && !isExistingTask && _messageHistory.Any())
                {
                    var userMsg = _messageHistory.Last(m => m.IsFromUser);
                    userMsg.IsTask = true;
                    userMsg.TaskId = effectiveTaskId;
                    userMsg.TaskStatus = "In Progress";
                }
                
                string taskStatus = "Unknown";
                if (isTask)
                {
                    taskStatus = result.TaskStatus ?? "Unknown";
                    if (taskStatus == "Unknown" && result.Response != null)
                    {
                        taskStatus = result.Response.Contains("COMPLETED", StringComparison.OrdinalIgnoreCase) ? "Completed" :
                                   result.Response.Contains("FAILED", StringComparison.OrdinalIgnoreCase) ? "Failed" :
                                   result.Response.Contains("CANCELLED", StringComparison.OrdinalIgnoreCase) ? "Cancelled" :
                                   "Completed";
                    }
                }
                
                _messageHistory.Add(new ChatMessage
                {
                    Text = result.Response ?? string.Empty,
                    IsFromUser = false,
                    Timestamp = DateTime.UtcNow,
                    IsTask = isTask,
                    TaskId = effectiveTaskId,
                    TaskStatus = taskStatus,
                    Parts = result.Parts
                });
            }
            else
            {
                _messageHistory.Add(new ChatMessage
                {
                    Text = $"Error: {result.ErrorMessage}",
                    IsFromUser = false,
                    Timestamp = DateTime.UtcNow,
                    IsTask = false,
                    TaskId = null,
                    TaskStatus = "Unknown"
                });
            }

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _messageHistory.Add(new ChatMessage
            {
                Text = $"Unexpected error: {ex.Message}",
                IsFromUser = false,
                Timestamp = DateTime.UtcNow,
                IsTask = false,
                TaskId = null,
                TaskStatus = "Unknown"
            });
        }
        finally
        {
            _isSendingMessage = false;
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !_isSendingMessage)
        {
            await SendMessage();
        }
    }

    private void OnAgentConnected()
    {
        _messageHistory.Clear();
        _messageText = string.Empty;
    }

    private void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        StateHasChanged();
    }

    private List<ChatMessage> GetAvailableTasks()
    {
        var excludedStatuses = new[] { 
            "completed", "cancelled", "canceled", "failed", 
            "rejected", "auth required", "unknown" 
        };
        
        var availableTasks = _messageHistory
            .Where(m => m.IsTask && !string.IsNullOrEmpty(m.TaskId))
            .GroupBy(m => m.TaskId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .Where(latestMessage => !excludedStatuses.Contains(latestMessage.TaskStatus.ToLower()))
            .OrderByDescending(m => m.Timestamp)
            .ToList();

        if (availableTasks.Any() && string.IsNullOrEmpty(_selectedTaskId))
        {
            _selectedTaskId = availableTasks.First().TaskId ?? string.Empty;
        }
        else if (!string.IsNullOrEmpty(_selectedTaskId) && availableTasks.All(t => t.TaskId != _selectedTaskId))
        {
            _selectedTaskId = availableTasks.FirstOrDefault()?.TaskId ?? string.Empty;
        }

        return availableTasks;
    }
}