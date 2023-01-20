using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Hubs;
public class DirectoryHub : Hub
{
    private const string ReceiveFolderContentMethod = "ReceiveFolderContent";

    // private const string UpdateDirectoryMethod = "UpdateCurrentDirectory";
    // private const string UpdateFilesMethod = "UpdateFiles";
    // private const string UpdateFoldersMethod = "UpdateFolders";

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
        await Clients.Group(clientIp).SendAsync(ReceiveFolderContentMethod, currentPath, files, folders);
    }
    public async Task FileSelected(string path)
    {
        const string message = "FileSelected";
        var clientIp = GetConnectionIp(Context);
        var relativePath = await _directoryService.GetRelativePath(path);
        await Clients.Group(clientIp).SendAsync(message, relativePath);
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
