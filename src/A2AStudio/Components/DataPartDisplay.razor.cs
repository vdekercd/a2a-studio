using A2AStudio.Models;
using System.Text.Json;

namespace A2AStudio.Components;

public partial class DataPartDisplay : ComponentBase
{
    [Parameter] public PartResult DataPart { get; set; } = new();
    
    private bool _showFormatted = true;

    private void ToggleFormat()
    {
        _showFormatted = !_showFormatted;
    }

    private bool HasValidJsonData()
    {
        try
        {
            if (string.IsNullOrEmpty(DataPart.JsonData)) return false;
            JsonDocument.Parse(DataPart.JsonData);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetDisplayJson()
    {
        try
        {
            if (string.IsNullOrEmpty(DataPart.JsonData)) return "{}";
            
            var jsonDoc = JsonDocument.Parse(DataPart.JsonData);
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = _showFormatted,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            return JsonSerializer.Serialize(jsonDoc.RootElement, options);
        }
        catch
        {
            return DataPart.JsonData ?? "{}";
        }
    }

    private Dictionary<string, string> GetDataSummary()
    {
        var summary = new Dictionary<string, string>();
        
        try
        {
            if (string.IsNullOrEmpty(DataPart.JsonData)) return summary;
            
            var jsonDoc = JsonDocument.Parse(DataPart.JsonData);
            var root = jsonDoc.RootElement;
            
            summary["Type"] = root.ValueKind.ToString();
            
            if (root.ValueKind == JsonValueKind.Object)
            {
                summary["Properties"] = root.EnumerateObject().Count().ToString();
                
                // Add some specific property info if available
                foreach (var prop in root.EnumerateObject().Take(5))
                {
                    var value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => $"\"{prop.Value.GetString()}\"",
                        JsonValueKind.Number => prop.Value.GetRawText(),
                        JsonValueKind.True or JsonValueKind.False => prop.Value.GetBoolean().ToString().ToLower(),
                        JsonValueKind.Array => $"Array[{prop.Value.GetArrayLength()}]",
                        JsonValueKind.Object => "Object{...}",
                        _ => prop.Value.ValueKind.ToString()
                    };
                    
                    summary[prop.Name] = value;
                }
            }
            else if (root.ValueKind == JsonValueKind.Array)
            {
                summary["Items"] = root.GetArrayLength().ToString();
            }
        }
        catch
        {
            summary["Error"] = "Unable to analyze JSON structure";
        }
        
        return summary;
    }
}