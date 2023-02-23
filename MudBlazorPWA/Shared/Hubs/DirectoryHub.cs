using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Services;

// ReSharper disable UnusedMember.Global

namespace MudBlazorPWA.Shared.Hubs;
public interface IHubClient
{
	Task ReceiveFolderContent(string currentPath, string[] files, string[] folders);
	Task FileSelected(string relativePath);

	Task ReceiveAllFolders(string[] folders);
	Task WindingCodesDbUpdated();

	Task CurrentWindingStopUpdated(WindingCode code);
}

public class DirectoryHub : Hub<IHubClient>
{
	#region Constructor
	private readonly IDirectoryService _directoryService;
	private readonly ILogger<DirectoryHub> _logger;
	private readonly IDataContext _dataContext;
	private WindingCode? _currentWindingStop;
	public DirectoryHub(IDirectoryService directoryService, ILogger<DirectoryHub> logger, IDataContext dataContext) {
		_directoryService = directoryService;
		_logger = logger;
		_dataContext = dataContext;
	}
	#endregion

	#region Hub Overrides
	public override async Task OnConnectedAsync() {
		var clientIp = GetConnectionIp(Context);
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);


		_logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
	}
	public override async Task OnDisconnectedAsync(Exception exception) {
		// remove the client from the group
		var clientIp = GetConnectionIp(Context);

		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);

		_logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
	}
	#endregion

	#region Directory Methods
	public async Task GetFolderContent(string? path = null) {
		var clientIp = GetConnectionIp(Context);
		var (currentPath, files, folders) = await _directoryService.GetFolderContent(path);
		await Clients.Group(clientIp).ReceiveFolderContent(currentPath, files, folders);
	}
	public async Task FileSelected(string path) {
		var clientIp = GetConnectionIp(Context);
		var relativePath = _directoryService.GetRelativePath(path);
		await Clients.Group(clientIp).FileSelected(relativePath);
	}
	public async Task GetAllFolders(string? path = null) {
		var clientIp = GetConnectionIp(Context);
		var folders = await _directoryService.GetFoldersInPath(path);
		await Clients.Group(clientIp).ReceiveAllFolders(folders);
	}
	public async Task<string> SaveWindingCodesDb(IEnumerable<WindingCode> windingCodes, bool syncDatabase) {
		var clientIp = GetConnectionIp(Context);
		// await _directoryService.ExportWindingCodesToJson(windingCodes, syncDatabase);
		if (syncDatabase)
			await _directoryService.UpdateDatabaseWindingCodes(windingCodes);

		await Clients.Group(clientIp).WindingCodesDbUpdated();
		return "From server: WindingCodes.json saved.";
	}
	public virtual Task<string> GetServerWindingDocsFolder() => Task.FromResult(AppConfig.BasePath);
	#endregion

	#region DataBase CRUD
	public async Task<IEnumerable<WindingCode>?> GetWindingCodes(Division? division = null) {
		var windingCodes = _dataContext.WindingCodes.AsQueryable();
		if (division != null) {
			windingCodes = windingCodes.Where(w => w.Division == division);
		}
		var result = await windingCodes.ToListAsync();
		return result.Any() ? result : null;
	}
	public async Task<WindingCode?> GetWindingCode(int id) {
		var windingCode = await _dataContext.WindingCodes.FirstOrDefaultAsync(e => e.Id == id);
		return windingCode ?? null;
	}
	public async Task<WindingCode?> UpdateWindingCode(string codeName, WindingCode windingCode) {
		if (codeName != windingCode.Name) {
			return null;
		}
		((DataContext)_dataContext).Entry(windingCode).State = EntityState.Modified;
		try {
			await _dataContext.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException) {
			if (!WindingCodeExists(codeName)) {
				return null;
			}
			else {
				throw;
			}
		}
		return windingCode;
	}
	public async Task<bool> CreateWindingCode(WindingCode windingCode) {
		if (WindingCodeExists(windingCode.Name)) {
			return false;
		}
		_dataContext.WindingCodes.Add(windingCode);
		await _dataContext.SaveChangesAsync();
		return true;
	}
	public async Task DeleteWindingCode(string codeName) {
		var windingCode = await _dataContext.WindingCodes.FirstOrDefaultAsync(e => e.Name == codeName);
		if (windingCode is null) {
			return;
		}
		_dataContext.WindingCodes.Remove(windingCode);
		await _dataContext.SaveChangesAsync();
	}
	private bool WindingCodeExists(string codeName) {
		return _dataContext.WindingCodes.Any(e => e.Name == codeName);
	}
	#endregion

	#region WindingStop Tracking
	public async void UpdateCurrentWindingStop(WindingCode code) {
		var clientIp = GetConnectionIp(Context);
		try {
			var windingCode = await _directoryService.GetWindingCodeDocuments(code);
			_currentWindingStop = windingCode;
			await Clients.Group(clientIp).CurrentWindingStopUpdated(windingCode);
		}
		catch (Exception e) {
			_logger.LogError("Error updating current winding stop => {Exception}", e.Message);

		}
	}
	public Task<WindingCode?> GetCurrentWindingStop() {
		return Task.FromResult(_currentWindingStop);
	}
	#endregion

	#region Static Methods
	private static string? GetConnectionIp(HubCallerContext context) {
		var connection = context.Features.Get<IHttpConnectionFeature>();
		return
			connection?
				.RemoteIpAddress?
				.ToString()
				.Replace("::ffff:", string.Empty);
	}
	#endregion
}
