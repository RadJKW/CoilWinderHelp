using Microsoft.AspNetCore.SignalR;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Hubs;
public class DirectoryHub : Hub
{
  private const string DirectoryChangedMethod = "DirectoryChanged";
  private const string ReceiveFolderContentMethod = "ReceiveFolderContent";
  private const string UpdateDirectoryMethod = "UpdateCurrentDirectory";
  private const string UpdateFilesMethod = "UpdateFiles";
  private const string UpdateFoldersMethod = "UpdateFolders";
  private readonly IDirectoryService _directoryService;

  public DirectoryHub(IDirectoryService directoryService)
  {
    _directoryService = directoryService;
  }

  public async Task GoToFolder(string currentPath, string folderName)
  {
    var newPath = await _directoryService.GoToFolder(currentPath, folderName);
    if (newPath != null)
    {
      await Clients.All.SendAsync(DirectoryChangedMethod, newPath);
    }
  }

  public async Task GoBack(string currentPath)
  {
    var newPath = await _directoryService.GoBack(currentPath);
    await Clients.All.SendAsync(DirectoryChangedMethod, newPath);
  }

  public async Task GetDirectoryContent(string path)
  {
    var (currentPath, files, folders) = await _directoryService.GetDirectoryContent(path);
    await Clients.All.SendAsync(ReceiveFolderContentMethod, currentPath, files, folders);
  }

  // ReSharper disable once UnusedMember.Global
  public async Task SetCurrentDirectory(string? path)
  {
    await _directoryService.SetCurrentDirectory(path ?? null);
  }


}
