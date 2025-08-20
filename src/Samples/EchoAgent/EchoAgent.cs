using A2A;
using System.Text.Json;

namespace EchoAgent;

public class EchoAgent
{
    private ITaskManager? _taskManager;

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
        taskManager.OnTaskCreated = ProcessTaskCreatedAsync;
    }

    private async Task ProcessTaskCreatedAsync(AgentTask agentTaskParams, CancellationToken cancellationToken)
    {
        var lastMessage = agentTaskParams.History!.Last();
        var messageText = lastMessage.Parts.OfType<TextPart>().First().Text;

        await _taskManager!.ReturnArtifactAsync(agentTaskParams.Id, new Artifact
        {
            Parts =
            [
                new TextPart
                {
                    Text = $"Echo: {messageText}"
                },
                new FilePart
                    { File = new FileWithUri { Uri = "https://s.w.org/images/core/emoji/16.0.1/svg/1f30a.svg" } }
            ]
        }, cancellationToken);

        await _taskManager!.UpdateStatusAsync(
            agentTaskParams.Id,
            TaskState.Completed,
            final: true,
            cancellationToken: cancellationToken);
    }

    private async Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        return await Task.FromResult<AgentCard>(new AgentCard()
        {
            Name = "Echo Agent Demo",
            Description = "A simple demonstration agent that echoes back any message it receives",
            Url = agentUrl,
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