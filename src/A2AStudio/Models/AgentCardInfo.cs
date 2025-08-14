namespace A2AStudio.Models;

public class AgentCardInfo
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Capabilities { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime? LastUpdated { get; set; }
    public bool IsConnected { get; set; }
}