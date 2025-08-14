namespace A2AStudio.Models;

public class AgentCardInfo
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public string? Url { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Capabilities { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public DateTime? LastUpdated { get; set; }
    public bool IsConnected { get; set; }
    
    // Validation properties
    public ValidationResult? ValidationResult { get; set; }
    public bool IsValid => ValidationResult?.IsValid != false;
    public bool HasValidationIssues => ValidationResult?.HasIssues == true;
}