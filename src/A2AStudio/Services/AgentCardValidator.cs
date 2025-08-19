namespace A2AStudio.Services;

public static class AgentCardValidator
{
    public static ValidationResult ValidateAgentCard(object? agentCard)
    {
        var result = new ValidationResult();
        
        if (agentCard == null)
        {
            result.AddError("AgentCard is null");
            return result;
        }
        
        if (agentCard is not AgentCard card)
        {
            result.AddError($"Object is not an AgentCard (type: {agentCard.GetType().Name})");
            return result;
        }
        
        // Required fields validation
        ValidateRequiredFields(card, result);
        
        // URL validation
        ValidateUrl(card, result);
        
        // Skills validation
        ValidateSkills(card, result);
        
        return result;
    }
    
    private static void ValidateRequiredFields(AgentCard agentCard, ValidationResult result)
    {
        // Name is required
        if (string.IsNullOrWhiteSpace(agentCard.Name))
        {
            result.AddError("Agent name is required");
        }
        
        // Description is recommended
        if (string.IsNullOrWhiteSpace(agentCard.Description))
        {
            result.AddWarning("Agent description is recommended for better discoverability");
        }
        
        // URL is required
        if (string.IsNullOrWhiteSpace(agentCard.Url))
        {
            result.AddError("Agent URL is required");
        }
        
        // Version is required
        if (string.IsNullOrWhiteSpace(agentCard.Version))
        {
            result.AddError("Agent version is required");
        }
    }
    
    private static void ValidateUrl(AgentCard agentCard, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(agentCard.Url))
            return;
            
        if (!Uri.TryCreate(agentCard.Url, UriKind.Absolute, out var agentUri))
        {
            result.AddError("Agent URL is not a valid absolute URI");
            return;
        }
        
        // Protocol should be HTTP/HTTPS for A2A
        if (agentUri.Scheme != "http" && agentUri.Scheme != "https")
        {
            result.AddError("Agent URL should use HTTP or HTTPS protocol");
        }
    }
    
    
    private static void ValidateSkills(AgentCard agentCard, ValidationResult result)
    {
        if (agentCard.Skills.Count == 0)
        {
            result.AddWarning("No skills defined - agent capabilities may not be discoverable");
            return;
        }
        
        var skillNames = new HashSet<string>();
        
        foreach (var skill in agentCard.Skills)
        {
            
            if (string.IsNullOrWhiteSpace(skill.Name))
            {
                result.AddError("Skill missing required 'Name' property");
            }
            else
            {
                if (!skillNames.Add(skill.Name))
                {
                    result.AddWarning($"Duplicate skill name '{skill.Name}' found");
                }
            }
            
            if (string.IsNullOrWhiteSpace(skill.Description))
            {
                result.AddWarning($"Skill '{skill.Name}' missing description");
            }
        }
    }
}