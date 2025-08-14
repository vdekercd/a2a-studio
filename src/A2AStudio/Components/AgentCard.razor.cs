using A2AStudio.Models;

namespace A2AStudio.Components;

public partial class AgentCard : ComponentBase
{
    [Parameter, EditorRequired]
    public AgentCardInfo Agent { get; set; } = null!;
    
    private bool showValidationDetails = false;

    private string GetInitials(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return "?";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{words[0][0]}{words[1][0]}".ToUpper();
        
        return name.Length >= 2 ? name.Substring(0, 2).ToUpper() : name[0].ToString().ToUpper();
    }
    
    private string GetValidationTooltip()
    {
        if (Agent.ValidationResult == null)
            return "No validation performed";
            
        var issues = new List<string>();
        
        if (Agent.ValidationResult.Errors.Count > 0)
            issues.Add($"{Agent.ValidationResult.Errors.Count} error(s)");
            
        if (Agent.ValidationResult.Warnings.Count > 0)
            issues.Add($"{Agent.ValidationResult.Warnings.Count} warning(s)");
            
        return $"Agent card validation: {string.Join(", ", issues)}";
    }
    
    private void ToggleValidationDetails()
    {
        showValidationDetails = !showValidationDetails;
        StateHasChanged();
    }
}