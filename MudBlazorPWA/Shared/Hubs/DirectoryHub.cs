using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Hubs;

public class DirectoryHub : Hub
{
    private const string DirectoryChangedMethod = "DirectoryChanged";

    private const string ReceiveFolderContentMethod = "ReceiveFolderContent";

    // private const string UpdateDirectoryMethod = "UpdateCurrentDirectory";
    // private const string UpdateFilesMethod = "UpdateFiles";
    // private const string UpdateFoldersMethod = "UpdateFolders";
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<DirectoryHub> _logger;
    public DirectoryHub(IDirectoryService directoryService, ILogger<DirectoryHub> logger)
    {
        _directoryService = directoryService;
        _logger = logger;
    }

//ReSharper disable UnusedMember.Global
    public async Task GoToFolder(string currentPath, string folderName)
    {
        var newPath = await _directoryService.GoToFolder(currentPath, folderName);
        if (newPath != null)
        {
            await Clients.All.SendAsync(DirectoryChangedMethod, newPath);
        }
        _logger.LogInformation("GoToFolder: {Path}", newPath);
    }

    public async Task GoBack(string currentPath)
    {
        var newPath = await _directoryService.GoBack(currentPath);
        await Clients.All.SendAsync(DirectoryChangedMethod, newPath);
    }

    public async Task GetFolderContent(string? path = null)
    {
        if (path == null)
        {
            var (currentPath, files, folders) = await _directoryService.GetFolderContent();
            await Clients.All.SendAsync(ReceiveFolderContentMethod, currentPath, files, folders);
        }
        else
        {
            var (currentPath, files, folders) = await _directoryService.GetFolderContent(path);
            await Clients.All.SendAsync(ReceiveFolderContentMethod, currentPath, files, folders);
        }
        _logger.LogInformation("GetFolderContent: {Path}", path);
    }

    public async Task FileSelected(string path)
    {
        var relativePath = await _directoryService.GetRelativePath(path);
        await Clients.All.SendAsync("FileSelected", path, relativePath);

        _logger.LogInformation("FileSelected: {Path}", path);
    }

    public async Task SetCurrentFolder(string? path)
    {
        await _directoryService.SetCurrentFolder(path);
        _logger.LogInformation("SetCurrentFolder: {Path}", path);
    }

//ReSharper enable UnusedMember.Global

}
