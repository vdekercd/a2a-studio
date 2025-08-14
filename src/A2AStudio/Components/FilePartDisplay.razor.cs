using A2AStudio.Models;

namespace A2AStudio.Components;

public partial class FilePartDisplay : ComponentBase
{
    [Parameter] public PartResult FilePart { get; set; } = new();

    private bool HasFileBytes() => FilePart.FileBytes != null && FilePart.FileBytes.Length > 0;
    private bool HasFileUri() => !string.IsNullOrEmpty(FilePart.FileUri);

    private bool IsImageSvg() => FilePart.MimeType == "image/svg+xml";
    
    private bool IsTextFile() => FilePart.MimeType?.StartsWith("text/") == true || 
                                FilePart.MimeType == "application/json" ||
                                FilePart.MimeType == "application/xml";
    
    private bool IsImageFile() => FilePart.MimeType?.StartsWith("image/") == true && !IsImageSvg();

    private string GetSvgContent()
    {
        if (FilePart.FileBytes == null) return string.Empty;
        return System.Text.Encoding.UTF8.GetString(FilePart.FileBytes);
    }

    private string GetTextContent()
    {
        if (FilePart.FileBytes == null) return string.Empty;
        return System.Text.Encoding.UTF8.GetString(FilePart.FileBytes);
    }

    private string GetImageDataUrl()
    {
        if (FilePart.FileBytes == null) return string.Empty;
        var base64 = Convert.ToBase64String(FilePart.FileBytes);
        return $"data:{FilePart.MimeType};base64,{base64}";
    }

    private string FormatFileSize(int bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private async Task DownloadFile()
    {
        if (FilePart.FileBytes == null || string.IsNullOrEmpty(FilePart.FileName))
            return;
        
        await Task.CompletedTask;
    }
}