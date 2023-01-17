using MudBlazorPWA.Shared.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MudBlazorPWA.Shared.Services;
public interface IDirectoryService
{
    string? CurrentDirectory { get; }
    Task<(string, string[], string[])> GetFolderContent();
    Task<(string, string[], string[])> GetFolderContent(string path);
    Task<string?> GoToFolder(string currentPath, string folderName);
    Task<string> GoBack(string currentPath);
    Task SetCurrentFolder(string? path);
}

public class DirectoryService : IDirectoryService
{
    private const string WindowsPath = @"B:\CoilWinderTraining-Edit\";
    private const string MacPath = @"/Users/jkw/WindingPractices/";
    private readonly IHubContext<DirectoryHub> _hubContext;
    private string RootDirectory { get; } = OperatingSystem.IsWindows() ? WindowsPath : MacPath;
    private string? _currentDirectory;

    public DirectoryService(IHubContext<DirectoryHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public string? CurrentDirectory
    {
        get => _currentDirectory ?? RootDirectory;
        set => _currentDirectory = value;
    }
    public Task<(string, string[], string[])> GetFolderContent()
    {
        var path = _currentDirectory ?? RootDirectory;

        var files = Directory.EnumerateFiles(path).Where(f => f.EndsWith(".mp4") || f.EndsWith(".pdf")).ToArray();
        var folders = Directory.GetDirectories(path);
        return Task.FromResult((path, files, folders));
    }

    public Task<(string, string[], string[])> GetFolderContent(string path)
    {
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
    public Task SetCurrentFolder(string? path)
    {
        if (path == null || !Path.GetRelativePath(path, RootDirectory).StartsWith(".."))

        {
            throw new ArgumentException("Invalid path. The path must be within the RootDirectory or its parent.");
        }
        _currentDirectory = path;
        return Task.CompletedTask;
    }

}
