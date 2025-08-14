namespace A2AStudio.Models;

public class PartResult
{
    public string Type { get; set; } = "text";
    public string? Text { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public byte[]? FileBytes { get; set; }
    public string? FileUri { get; set; }
    public string? JsonData { get; set; }
}