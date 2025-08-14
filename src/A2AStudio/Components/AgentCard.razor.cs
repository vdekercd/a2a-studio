using A2AStudio.Models;

namespace A2AStudio.Components;

public partial class AgentCard : ComponentBase
{
    [Parameter, EditorRequired]
    public AgentCardInfo Agent { get; set; } = null!;

    private string GetInitials(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return "?";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{words[0][0]}{words[1][0]}".ToUpper();
        
        return name.Length >= 2 ? name.Substring(0, 2).ToUpper() : name[0].ToString().ToUpper();
    }
}