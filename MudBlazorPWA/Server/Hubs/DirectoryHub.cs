// ReSharper disable UnusedMember.Global
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MudBlazorPWA.Server.Extensions;
using MudBlazorPWA.Shared.Data;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Server.Hubs;
public interface IHubClient
{
	Task ReceiveFolderContent(string currentPath, string[] files, string[] folders);
	Task FileSelected(string relativePath);

	Task ReceiveAllFolders(string[] folders);
	Task WindingCodesDbUpdated();

	Task CurrentWindingStopUpdated(IWindingCode code);
}

/// <inheritdoc />
public class DirectoryHub : Hub<IHubClient>
{
	#region Constructor
	private readonly IDirectoryService _directoryService;
	private readonly ILogger<DirectoryHub> _logger;
	private readonly IDataContext _dataContext;
	private IWindingCode? _currentWindingStop;
	public DirectoryHub(IDirectoryService directoryService,
		ILogger<DirectoryHub> logger,
		IDataContext dataContext) {
		_directoryService = directoryService;
		_logger = logger;
		_dataContext = dataContext;
	}
	private DbSet<Z80WindingCode> Z80WindingCodes => _dataContext.Z80WindingCodes;
	private DbSet<PcWindingCode> PcWindingCodes => _dataContext.PcWindingCodes;
	#endregion

	#region Hub Overrides
	public override async Task OnConnectedAsync() {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		if (clientIp is null) {
			_logger.LogWarning("Client IP is null");
			return;
		}

		// Get the name of this hub using reflection
		string hubName = GetType().Name;

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
	public override async Task OnDisconnectedAsync(Exception? exception) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		string hubName = GetType().Name;
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
		string hubName = GetType().Name;
		var clients = HubExtensions.ActiveConnections[hubName].Select(x => x.ip).ToList();
		// use console write line to see each entry in the dictionary in the console
		foreach (string ip in clients) {
			Console.WriteLine($"IP: {ip}");
		}
		return Task.FromResult(clients);
	}
	public async Task<List<string>> GetCallbackMethods() {
		Type hubType = GetType();
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
	public async Task<List<string>> ListVideoFiles(string? path) {
		var result = await _directoryService.ListVideoFiles(path);
		return result;
	}
	public async Task<List<string>> ListPdfFiles(string? path) {
		// var clientIP = HubExtensions.GetConnectionIp(Context);
		var files = await _directoryService.ListPdfFiles(path);
		return files;
	}
	public Task<List<string>> ListMediaFiles(string? path) {
		/*var files = await _directoryService.ListMediaFiles(path);
		return files;*/
		throw new NotImplementedException();
	}
	public async Task GetFolderContent(string? path = null) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		(string currentPath, string[] files, string[] folders) = await _directoryService.GetFolderContent(path);
		if (clientIp != null)
			await Clients.Group(clientIp).ReceiveFolderContent(currentPath, files, folders);
	}
	public async Task FileSelected(string path) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		string relativePath = _directoryService.GetRelativePath(path);
		await Clients.Group(clientIp!).FileSelected(relativePath);
	}
	public async Task<IEnumerable<string>> GetAllFolders(string? path) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		string[] folders = await _directoryService.GetFoldersInPath(path);
		await Clients.Group(clientIp!).ReceiveAllFolders(folders.ToArray());
		return folders;
	}
 #pragma warning disable CA1822
	public string GetServerWindingDocsFolder() => AppConfig.BasePath;
 #pragma warning restore CA1822
	#endregion

	#region DataBase CRUD

	public async Task<IEnumerable<IWindingCode>?> GetWindingCodes(Division? division, WindingCodeType windingCodeType) {

		var windingCodes = windingCodeType switch {
			WindingCodeType.Z80 => _dataContext.Z80WindingCodes.AsQueryable(),
			WindingCodeType.Pc => _dataContext.PcWindingCodes.AsQueryable(),
			_ => new List<IWindingCode>().AsQueryable()
		};
		var results = division is null
			? await windingCodes.ToListAsync()
			: await windingCodes.Where(w => w.Division == division).ToListAsync();
		_logger.LogInformation("Found {Count} winding codes for Type {WindingCodeType} and Division {Division}", results.Count, windingCodeType, division);
		return results.Any() ? results : null;
	}

	public async Task<IWindingCode?> GetWindingCode(int codeId, WindingCodeType windingCodeType) {
		IWindingCode? windingCode;
		switch (windingCodeType) {
			case WindingCodeType.Z80: {
				windingCode = await _dataContext.Z80WindingCodes
					.FirstOrDefaultAsync(w => w.Id == codeId);
				return windingCode ?? null;
			}
			case WindingCodeType.Pc: {
				windingCode = await _dataContext.PcWindingCodes
					.FirstOrDefaultAsync(w => w.Id == codeId);
				return windingCode ?? null;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(windingCodeType), windingCodeType, null);
		}
	}


	public async Task<bool> CreateWindingCode(IWindingCode windingCode, WindingCodeType windingCodeType) {
		if (WindingCodeExists(windingCode.Id, windingCodeType)) {
			return false;
		}
		switch (windingCodeType) {
			case WindingCodeType.Z80: {
				await _dataContext.Z80WindingCodes.AddAsync((Z80WindingCode)windingCode);
				break;
			}
			case WindingCodeType.Pc: {
				await _dataContext.PcWindingCodes.AddAsync((PcWindingCode)windingCode);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(windingCodeType), windingCodeType, null);
		}
		await _dataContext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> UpdateWindingCode(IWindingCode windingCode, WindingCodeType windingCodeType)
	{
		if (!WindingCodeExists(windingCode.Id, windingCodeType))
		{
			return false;
		}

		var dbContext = (DataContext)_dataContext;
		var mapper = new WindingCodeMapper();

		switch (windingCodeType) {
			case WindingCodeType.Z80: {
				Z80WindingCode updatedCode = mapper.MapToZ80(windingCode);
				dbContext.Z80WindingCodes.Update(updatedCode);
				break;
			}
			case WindingCodeType.Pc: {
				PcWindingCode updatedCode = mapper.MapToPc(windingCode);
				dbContext.PcWindingCodes.Update(updatedCode);
				break;
			}
			default:
				return false;
		}

		await dbContext.SaveChangesAsync();
		await Clients.All.WindingCodesDbUpdated();
		return true;
	}


	public async Task DeleteWindingCode(int codeId, WindingCodeType windingCodeType) {

		if (!WindingCodeExists(codeId, windingCodeType)) {
			return;
		}

		switch (windingCodeType) {
			case WindingCodeType.Z80: {
				Z80WindingCode? windingCode = await _dataContext.Z80WindingCodes.FindAsync(codeId);
				_dataContext.Z80WindingCodes.Remove(windingCode!);
				break;
			}
			case WindingCodeType.Pc: {
				PcWindingCode? windingCode = await _dataContext.PcWindingCodes.FindAsync(codeId);
				_dataContext.PcWindingCodes.Remove(windingCode!);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(windingCodeType), windingCodeType, null);
		}
	}
	private bool WindingCodeExists(int codeId, WindingCodeType windingCodeType)
	{
		return windingCodeType switch
		{
			WindingCodeType.Z80 => Z80WindingCodes.Any(e => e.Id == codeId),
			WindingCodeType.Pc => PcWindingCodes.Any(e => e.Id == codeId),
			_ => false,
		};
	}

	#endregion

	#region WindingStop Tracking
	public async void UpdateCurrentWindingStop(int codeId, WindingCodeType windingCodeType) {
		string? clientIp = HubExtensions.GetConnectionIp(Context);
		_currentWindingStop = await GetWindingCode(codeId, windingCodeType);
		if (_currentWindingStop is null) {
			_logger.LogWarning("Could not find WindingCode with Id {CodeId} and Type {WindingCodeType}", codeId, windingCodeType);
			return;
		}
		await Clients.Group(clientIp!).CurrentWindingStopUpdated(_currentWindingStop);
	}
	public Task<IWindingCode?> GetCurrentWindingStop() {
		return Task.FromResult(_currentWindingStop);
	}
	#endregion

}
