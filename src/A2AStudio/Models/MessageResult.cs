namespace A2AStudio.Models;

public class MessageResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Response { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<PartResult> Parts { get; set; } = [];
    public bool IsTask { get; set; }
    public string? TaskId { get; set; }
    public string? TaskStatus { get; set; }
}