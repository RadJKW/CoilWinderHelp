using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Services;

namespace MudBlazorPWA.Shared.Hubs;
public interface IHubClient
{
    Task ReceiveFolderContent(string currentPath, string[] files, string[] folders);
    Task FileSelected(string relativePath);

    Task ReceiveAllFolders(string[] folders);
    Task WindingCodesDbUpdated();
}
public class DirectoryHub : Hub<IHubClient>
{
    #region Constructor
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<DirectoryHub> _logger;
    private readonly IDataContext _dataContext;
    public DirectoryHub(IDirectoryService directoryService, ILogger<DirectoryHub> logger, IDataContext dataContext)
    {
        _directoryService = directoryService;
        _logger = logger;
        _dataContext = dataContext;
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

    public async Task<string> SaveWindingCodesDb(IEnumerable<WindingCode> windingCodes, bool syncDatabase)
    {
        var clientIp = GetConnectionIp(Context);
        await _directoryService.ExportWindingCodesToJson(windingCodes, syncDatabase);
        await Clients.Group(clientIp).WindingCodesDbUpdated();
        return "From server: WindingCodes.json saved.";
    }
    #endregion

    #region DataBase CRUD
    public async Task<IEnumerable<WindingCode>?> GetWindingCodes()
    {
        // return the codes as a list if any exist
        var windingCodes = await _dataContext.WindingCodes.ToListAsync();
        return windingCodes.Any() ? windingCodes : null;
    }

    public async Task<WindingCode?> GetWindingCode(string codeName)
    {
        var windingCode = await _dataContext.WindingCodes.FirstOrDefaultAsync( e => e.Name == codeName);

        return windingCode ?? null;
    }

    public async Task<WindingCode?> UpdateWindingCode(string codeName, WindingCode windingCode)
    {
        if (codeName != windingCode.Name)
        {
            return null;
        }
        ((DataContext)_dataContext).Entry(windingCode).State = EntityState.Modified;
        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WindingCodeExists(codeName))
            {
                return null;
            }
            else
            {
                throw;
            }
        }
        return windingCode;
    }

    /*public async Task PutWindingCode(string codeName, WindingCode windingCode)
    {
    }*/

    public async Task<bool> CreateWindingCode(WindingCode windingCode) {

        if (WindingCodeExists(windingCode.Name!))
        {
            return false;
        }
        _dataContext.WindingCodes.Add(windingCode);
        await _dataContext.SaveChangesAsync();
        return true;
    }

    public async Task DeleteWindingCode(string codeName)
    {
        var windingCode = await _dataContext.WindingCodes.FirstOrDefaultAsync( e => e.Name == codeName);
        if (windingCode is null)
        {
            return;
        }
        _dataContext.WindingCodes.Remove(windingCode);
        await _dataContext.SaveChangesAsync();
    }

    private bool WindingCodeExists(string codeName)
    {
        return _dataContext.WindingCodes.Any( e => e.Name == codeName);
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
