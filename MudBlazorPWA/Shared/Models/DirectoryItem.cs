namespace MudBlazorPWA.Shared.Models;
public class DirectoryItem
{
  public string? Name { get; set; }
  public string? Path { get; set; }
  public bool IsDirectory { get; set; }
  public string? Extension { get; set; }
  public DateTime LastWriteTime { get; set; }
}
