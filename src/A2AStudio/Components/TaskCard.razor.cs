using A2AStudio.Models;

namespace A2AStudio.Components;

public partial class TaskCard : ComponentBase
{
    [Parameter] public ChatMessage TaskMessage { get; set; } = new();
    [Parameter] public List<ChatMessage> TaskMessages { get; set; } = new();

    private string GetStatusClass()
    {
        return TaskMessage.TaskStatus.ToLower() switch
        {
            "submitted" => "status-submitted",
            "working" => "status-working",
            "input required" => "status-input-required",
            "completed" => "status-completed",
            "cancelled" => "status-cancelled",
            "failed" => "status-failed",
            "rejected" => "status-rejected",
            "auth required" => "status-auth-required",
            "unknown" => "status-unknown",
            _ => "status-unknown"
        };
    }

    private string GetStatusIcon()
    {
        return TaskMessage.TaskStatus.ToLower() switch
        {
            "submitted" => "fas fa-paper-plane",
            "working" => "fas fa-spinner fa-spin",
            "input required" => "fas fa-keyboard",
            "completed" => "fas fa-check-circle",
            "cancelled" => "fas fa-ban",
            "failed" => "fas fa-times-circle",
            "rejected" => "fas fa-times-circle",
            "auth required" => "fas fa-lock",
            "unknown" => "fas fa-question-circle",
            _ => "fas fa-question-circle"
        };
    }
}