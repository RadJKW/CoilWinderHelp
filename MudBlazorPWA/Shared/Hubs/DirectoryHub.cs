using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Hubs;
public interface IHubClient
{
    Task ReceiveFolderContent(string currentPath, string[] files, string[] folders);
    Task FileSelected(string relativePath);

    Task ReceiveAllFolders(string[] folders);


}
public class DirectoryHub : Hub<IHubClient>
{
    #region Constructor
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<DirectoryHub> _logger;
    public DirectoryHub(IDirectoryService directoryService, ILogger<DirectoryHub> logger)
    {
        _directoryService = directoryService;
        _logger = logger;
    }
    #endregion

    #region Hub Overrides
    public override async Task OnConnectedAsync()
    {
        var clientIp = GetConnectionIp(Context);
        if (clientIp is null)
        {
            _logger.LogWarning("Client IP is null");
            return;
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);


        _logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // remove the client from the group
        var clientIp = GetConnectionIp(Context);

        if (clientIp is null)
        {
            _logger.LogWarning("Client IP is null");
            return;
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);

        _logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
    }
    #endregion

    #region Public Methods
    public async Task GetFolderContent(string? path = null)
    {
        var clientIp = GetConnectionIp(Context);
        var (currentPath, files, folders) = await _directoryService.GetFolderContent(path);
        await Clients.Group(clientIp).ReceiveFolderContent( currentPath, files, folders);
    }
    public async Task FileSelected(string path)
    {
        var clientIp = GetConnectionIp(Context);
        var relativePath = await _directoryService.GetRelativePath(path);
        await Clients.Group(clientIp).FileSelected(relativePath);
    }

    public async Task GetAllFolders(string? path = null){
        var clientIp = GetConnectionIp(Context);
        var folders = await _directoryService.GetFoldersInPath(path);
        await Clients.Group(clientIp).ReceiveAllFolders(folders);
    }
    #endregion

    private static string? GetConnectionIp(HubCallerContext context)
    {
        var connection = context.Features.Get<IHttpConnectionFeature>();
        return
            connection?
                .RemoteIpAddress?
                .ToString()
                .Replace("::ffff:", string.Empty);
    }
}
