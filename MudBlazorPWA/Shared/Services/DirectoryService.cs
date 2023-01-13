using MudBlazorPWA.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
  public string RootDirectory { get; set; }
  Task<(string, string[], string[])> GetDirectoryContent(string path);
  Task<string?> GoToFolder(string currentPath, string folderName);
  Task<string?> GoBack(string currentPath);
}

public  class DirectoryService : IDirectoryService
{
  private readonly IHubContext<DirectoryHub> _hubContext;

  public DirectoryService(IHubContext<DirectoryHub> hubContext)
  {
    _hubContext = hubContext;
  }

  public string RootDirectory { get; set; } = @"B:\CoilWinderTraining-Edit\";
  public Task<(string, string[], string[])> GetDirectoryContent(string path)
  {
    // only display files that end in .mp4 or .pdf

    var files = Directory.EnumerateFiles(path).Where (f => f.EndsWith(".mp4") || f.EndsWith(".pdf")).ToArray();
    var folders = Directory.GetDirectories(path);
    return Task.FromResult((path, files, folders));
  }

  public async Task<string?> GoToFolder(string currentPath, string folderName)
  {
    // check if the folder exists in the current path
    string newPath = Path.Combine(currentPath, folderName);
    if (!Directory.Exists(newPath))
    {
      return null;
    }
    // invoke the DirectoryChanged method on all connected clients
    await _hubContext.Clients.All.SendAsync("DirectoryChanged", newPath);

    // return the new path
    return newPath;
  }

  public async Task<string?> GoBack(string currentPath)
  {
    // check if the current path is a root directory
    if (Path.GetPathRoot(currentPath) == currentPath)
    {
      return null;
    }

    // get the parent directory
    var parentDirectory = Directory.GetParent(currentPath);

    if (parentDirectory == null)
    {
      return null;
    }

    // do some additional logic such as user authentication
    // ...

    // invoke the DirectoryChanged method on all connected clients
    await _hubContext.Clients.All.SendAsync("DirectoryChanged", parentDirectory.FullName);

    // return the new path
    return parentDirectory.FullName;
  }

}
