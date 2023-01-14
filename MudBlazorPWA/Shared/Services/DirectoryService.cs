using MudBlazorPWA.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
    Task<(string, string[], string[])> GetDirectoryContent(string path);
    Task<string?> GoToFolder(string currentPath, string folderName);
    Task<string> GoBack(string currentPath);
    Task SetCurrentDirectory(string? path);
}

public class DirectoryService : IDirectoryService
{
    private const string WindowsPath = @"B:\CoilWinderTraining-Edit\";
    private const string MacPath = @"/Users/jkw/WindingPractices/";
    private readonly IHubContext<DirectoryHub> _hubContext;

    public DirectoryService(IHubContext<DirectoryHub> hubContext)
    {
        _hubContext = hubContext;
    }

    private string? RootDirectory { get; set; }
    public Task<(string, string[], string[])> GetDirectoryContent(string path)
    {
        // only display files that end in .mp4 or .pdf

        var files = Directory.EnumerateFiles(path).Where(f => f.EndsWith(".mp4") || f.EndsWith(".pdf")).ToArray();
        var folders = Directory.GetDirectories(path);
        return Task.FromResult((path, files, folders));
    }

    public async Task<string?> GoToFolder(string currentPath, string folderName)
    {
        // check if the folder exists in the current path
        var newPath = Path.Combine(currentPath, folderName);
        if (!Directory.Exists(newPath))
        {
            return null;
        }
        // invoke the DirectoryChanged method on all connected clients
        await _hubContext.Clients.All.SendAsync("DirectoryChanged", newPath);

        // return the new path
        return newPath;
    }

    public Task<string> GoBack(string currentPath)
    {
        var parentDirectory = Directory.GetParent(currentPath);
        return Task.FromResult(parentDirectory == null ? currentPath : parentDirectory.FullName);

    }
    public async Task SetCurrentDirectory(string? path)
    {
        if (path is not null)
            RootDirectory = path;
        if (path is null)
            RootDirectory = OperatingSystem.IsWindows() ? WindowsPath : MacPath;
        await _hubContext.Clients.All.SendAsync("DirectoryChanged", RootDirectory);
    }

}
