using A2AStudio.Models;

namespace A2AStudio.Components;

public partial class AgentCard : ComponentBase
{
    [Parameter, EditorRequired]
    public AgentCardInfo Agent { get; set; } = null!;
    
    [Inject]
    private IA2AService A2AService { get; set; } = null!;
    
    private bool showValidationDetails = false;
    private bool showJsonDetails = false;
    private string? agentCardJson;

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
    
    private void ToggleJsonDetails()
    {
        showJsonDetails = !showJsonDetails;
        
        if (showJsonDetails && agentCardJson == null)
        {
            agentCardJson = A2AService.GetAgentCardJson();
        }
        
        StateHasChanged();
    }
    
    private string FormatJsonWithSyntaxHighlighting(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;
        
        // Use regex for better JSON syntax highlighting with theme-aware CSS classes
        var highlighted = json;
        
        // Highlight strings (keys and values) - avoid conflicts by using unique markers first
        highlighted = System.Text.RegularExpressions.Regex.Replace(highlighted, 
            @"""([^""\\]|\\.)*""", 
            @"<span class='json-string'>$&</span>");
        
        // Highlight numbers
        highlighted = System.Text.RegularExpressions.Regex.Replace(highlighted, 
            @"\b-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?\b", 
            @"<span class='json-number'>$&</span>");
        
        // Highlight boolean values
        highlighted = System.Text.RegularExpressions.Regex.Replace(highlighted, 
            @"\b(true|false)\b", 
            @"<span class='json-boolean'>$1</span>");
        
        // Highlight null
        highlighted = System.Text.RegularExpressions.Regex.Replace(highlighted, 
            @"\bnull\b", 
            @"<span class='json-null'>$&</span>");
        
        // Highlight structural characters - do this last to avoid regex conflicts
        highlighted = highlighted
            .Replace(":", "<span class='json-punctuation'>:</span>")
            .Replace(",", "<span class='json-punctuation'>,</span>")
            .Replace("{", "<span class='json-brace'>{</span>")
            .Replace("}", "<span class='json-brace'>}</span>")
            .Replace("[", "<span class='json-bracket'>[</span>")
            .Replace("]", "<span class='json-bracket'>]</span>");
        
        return highlighted;
    }
}