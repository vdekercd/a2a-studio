using A2AStudio.Models;

namespace A2AStudio.Services;

public interface IA2AService
{
    Task<ConnectionResult> ConnectToAgentAsync(string agentUrl);
    Task<MessageResult> SendMessageAsync(string message, string? taskId, IEnumerable<string>? previousMessages);
    bool IsConnected { get; }
    AgentCardInfo? ConnectedAgent { get; }
    string? GetAgentCardJson();
    void Disconnect();
}

