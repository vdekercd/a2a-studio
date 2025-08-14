namespace A2AStudio.Models;

public class ConnectionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public AgentCardInfo? AgentCard { get; set; }
}