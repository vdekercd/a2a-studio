
namespace A2AStudio.Models;

public class ChatMessage
{
    public string Text { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsTask { get; set; }
    public string? TaskId { get; set; }
    public string TaskStatus { get; set; } = "Unknown";
    public List<PartResult> Parts { get; set; } = new();
}