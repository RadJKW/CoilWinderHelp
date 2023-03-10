using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
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

/// <inheritdoc />
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
		var clientIp = HubExtensions.GetConnectionIp(Context);
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}

		// Get the name of this hub using reflection
		var hubName = this.GetType().Name;

		// Add the connection to the group
		await Groups.AddToGroupAsync(Context.ConnectionId, groupName: clientIp);

		// Add the connection to the list of active connections
		if (!HubExtensions.ActiveConnections.TryGetValue(hubName, out var connections)) {
			connections = new List<(string, string)>();
			HubExtensions.ActiveConnections.Add(hubName, connections);
		}
		connections.Add((clientIp, Context.ConnectionId));

		_logger.LogInformation("Client {ClientId} connected to group {GroupName}", Context.ConnectionId, clientIp);
	}
	public override async Task OnDisconnectedAsync(Exception exception) {
		var clientIp = HubExtensions.GetConnectionIp(Context);
		var hubName = GetType().Name;
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName: clientIp);
		HubExtensions.ActiveConnections[hubName].RemoveAll(x => x.Item2 == Context.ConnectionId);
		_logger.LogInformation("Client {ClientIpAddress} disconnected from group {GroupName}", clientIp, clientIp);
	}
	#endregion
	public Task<List<string>> GetConnectedClients() {
		// var clientIp = HubExtensions.GetConnectionIp(Context);
		var hubName = GetType().Name;
		var clients = HubExtensions.ActiveConnections[hubName].Select(x => x.ip).ToList();
		// use console write line to see each entry in the dictionary in the console
		foreach (var ip in clients) {
			Console.WriteLine($"IP: {ip}");
		}
		return Task.FromResult(clients);
	}
	public async Task<List<string>> GetCallbackMethods() {
		var hubType = GetType();
		var methods = hubType.GetMethods()
			.Where(m =>
				(m.Attributes & MethodAttributes.Virtual) == 0 &&// exclude overridden methods
				m.ReturnType == typeof(Task) ||// include public Task methods
				(m.ReturnType == typeof(void) && m.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)// include public async void methods
			)
			.Select(m => m.Name)
			.ToList();
		return await Task.FromResult(methods);
	}

	#region Directory Methods
	public async Task<IEnumerable> ListPdfFiles(string? path) {
		// var clientIP = HubExtensions.GetConnectionIp(Context);
		var files = await _directoryService.ListPdfFiles(path);
		return files;
	}
	public async Task GetFolderContent(string? path = null) {
		var clientIp = HubExtensions.GetConnectionIp(Context);
		var (currentPath, files, folders) = await _directoryService.GetFolderContent(path);
		await Clients.Group(clientIp).ReceiveFolderContent(currentPath, files, folders);
	}
	public async Task FileSelected(string path) {
		var clientIp = HubExtensions.GetConnectionIp(Context);
		var relativePath = _directoryService.GetRelativePath(path);
		await Clients.Group(clientIp).FileSelected(relativePath);
	}
	public async Task<IEnumerable<string>> GetAllFolders(string? path){
		var clientIp = HubExtensions.GetConnectionIp(Context);
		var folders = await _directoryService.GetFoldersInPath(path);
		await Clients.Group(clientIp).ReceiveAllFolders(folders.ToArray());
		return folders;
	}
	public async Task<string> SaveWindingCodesDb(IEnumerable<WindingCode> windingCodes, bool syncDatabase) {
		var clientIp = HubExtensions.GetConnectionIp(Context);
		// await _directoryService.ExportWindingCodesToJson(windingCodes, syncDatabase);
		if (syncDatabase)
			await _directoryService.UpdateDatabaseWindingCodes(windingCodes);

		await Clients.Group(clientIp).WindingCodesDbUpdated();
		return "From server: WindingCodes.json saved.";
	}
 #pragma warning disable CA1822
	public Task<string> GetServerWindingDocsFolder() => Task.FromResult(AppConfig.BasePath);
 #pragma warning restore CA1822
	#endregion

	#region DataBase CRUD
	public async Task<IEnumerable<WindingCode>?> GetWindingCodes(Division? division) {
		var windingCodes = _dataContext.WindingCodes.AsQueryable();
		if (division != null) {
			windingCodes = windingCodes.Where(w => w.Division == division);
		}
		var result = await windingCodes.ToListAsync();
		return result.Any() ? result : null;
	}
	public async Task<WindingCode?> GetWindingCode(int codeId) {
		var windingCode = await _dataContext.WindingCodes.FirstOrDefaultAsync(e => e.Id == codeId);
		return windingCode ?? null;
	}
	public async Task<bool> UpdateWindingCode(WindingCode windingCode) {
		// use the code.Id to check if the code exists in the database
		if (!WindingCodeExists(windingCode.Id)) {
			return false;
		}
		await using var dbContext = (DataContext)_dataContext;
		dbContext.Entry(windingCode).State = EntityState.Modified;
		await dbContext.SaveChangesAsync();
		await Clients.All.WindingCodesDbUpdated();
		return true;
	}
	public async Task<bool> CreateWindingCode(WindingCode windingCode) {
		if (WindingCodeExists(windingCode.Id)) {
			return false;
		}
		_dataContext.WindingCodes.Add(windingCode);
		await _dataContext.SaveChangesAsync();
		return true;
	}
	public async Task DeleteWindingCode(int codeId) {
		var windingCode = await _dataContext.WindingCodes.FindAsync(codeId);
		if (windingCode == null) {
			return;
		}
		_dataContext.WindingCodes.Remove(windingCode);
		await _dataContext.SaveChangesAsync();
	}
	private bool WindingCodeExists(int codeId) {
		return _dataContext.WindingCodes.Any(e => e.Id == codeId);
	}
	#endregion

	#region WindingStop Tracking
	public async void UpdateCurrentWindingStop(WindingCode code) {
		var clientIp = HubExtensions.GetConnectionIp(Context);
		try {
			code.FolderPath = AppConfig.BasePath + code.FolderPath;
			var windingCode = await _directoryService.GetWindingCodeDocuments(code);
			_currentWindingStop = windingCode;
			await Clients.Group(clientIp).CurrentWindingStopUpdated(windingCode);
			//await Clients.All.CurrentWindingStopUpdated(windingCode);
		}
		catch (Exception e) {
			_logger.LogError("Error updating current winding stop => {Exception}", e.Message);
		}
	}
	public Task<WindingCode?> GetCurrentWindingStop() {
		return Task.FromResult(_currentWindingStop);
	}
	#endregion

}
