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
public class DirectoryHub : Hub<IHubClient> {
	#region Constructor
	private readonly ILogger<DirectoryHub> _logger;
	private readonly IDataContext _dataContext;
	private WindingCode? _currentWindingStop;
	public DirectoryHub(ILogger<DirectoryHub> logger,
		IDataContext dataContext) {
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

	#region Code Testing
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
		var hubType = GetType();
		var methods = hubType.GetMethods()
			.Where(
			m =>
				(m.Attributes & MethodAttributes.Virtual) == 0 &&// exclude overridden methods
				m.ReturnType == typeof(Task) ||// include public Task methods
				(m.ReturnType == typeof(void) && m.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null)// include public async void methods
			)
			.Select(m => m.Name)
			.ToList();
		return await Task.FromResult(methods);
	}

	#endregion

	#region Directory Methods
	public async Task<DirectoryNode> BuildDirectorySnapshot(string? path) {
		var dirInfo = new DirectoryInfo(path ?? AppConfig.BasePath);

		var node = new DirectoryNode(dirInfo.Name, dirInfo.FullName.Remove(0, AppConfig.BasePath.Length));

		foreach (var subdir in dirInfo.GetDirectories()) {
			node.Folders.Add(await BuildDirectorySnapshot(subdir.FullName));
		}

		foreach (var file in dirInfo.GetFiles()) {
			// return if the file type is not in AppConfig.AllowedFileTypes
			if (!AppConfig.AllowedFileTypes.Contains(file.Extension)) continue;

			var slicedPath = file.FullName.Remove(0, AppConfig.BasePath.Length);
			node.Files.Add(new(file.Name, slicedPath));
		}

		return node;
	}

	/// <summary>
	/// returns the static path to the server's winding docs folder
	/// </summary>
	/// <returns>Path on Server</returns>
	public string GetServerWindingDocsFolder() => AppConfig.BasePath;
	#endregion

	#region DataBase CRUD
	public async Task<IEnumerable<WindingCode>?> GetWindingCodes(Division? division, WindingCodeType windingCodeType) {
		var windingCodes = windingCodeType switch {
			WindingCodeType.Z80 => _dataContext.Z80WindingCodes.AsQueryable(),
			WindingCodeType.Pc => _dataContext.PcWindingCodes.AsQueryable(),
			_ => new List<WindingCode>().AsQueryable()
		};
		// fetch the latest winding codes from the database
		var results = division is null
			? await windingCodes.ToListAsync()
			: await windingCodes.Where(w => w.Division == division).ToListAsync();
		_logger.LogInformation("Found {Count} winding codes for Type {WindingCodeType} and Division {Division}", results.Count, windingCodeType, division);
		// foreach code in WindingCodes, IF the code.media.pdf is null, then use the code.FolderPath if not null to get the pdf
		// from the directory. // repeat for code.media.video if null.
		/*var dbContext = (DataContext)_dataContext;

		foreach (var code in results) {
			var hasChanges = false;
			if (code.FolderPath is null) continue;
			if (code.Media.Pdf is not null) continue;
			var dirInfo = new DirectoryInfo(AppConfig.BasePath + code.FolderPath);
			var pdfFile = dirInfo.GetFiles().FirstOrDefault(x => x.Extension == ".pdf");
			if (pdfFile is not null) {

				code.Media.Pdf = pdfFile.FullName.Remove(0, AppConfig.BasePath.Length);
				hasChanges = true;
			}
			if (code.Media.Video is not null) continue;
			var videoFile = dirInfo.GetFiles().FirstOrDefault(x => x.Extension == ".mp4");
			if (videoFile is not null) {
				code.Media.Video = videoFile.FullName.Remove(0, AppConfig.BasePath.Length);
				hasChanges = true;
			}

			if (!hasChanges) continue;
			_logger.LogInformation("Updating media for code {Code}", code.Code);
			dbContext.Update(code);
			await dbContext.SaveChangesAsync();
		}*/


		return results.Any()
			? results
			: null;
	}
	// ReSharper disable once MemberCanBePrivate.Global
	public async Task<WindingCode?> GetWindingCode(int codeId, WindingCodeType windingCodeType) {
		WindingCode? windingCode;
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

	public async Task<bool> CreateWindingCode(WindingCode windingCode, WindingCodeType windingCodeType) {
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
	public async Task<bool> UpdateWindingCode(WindingCode windingCode, WindingCodeType windingCodeType) {
		if (!WindingCodeExists(windingCode.Id, windingCodeType)) {
			return false;
		}

		var dbContext = (DataContext)_dataContext;
		var mapper = new WindingCodeMapper();

		switch (windingCodeType) {
			case WindingCodeType.Z80: {
				var updatedCode = mapper.MapToZ80(windingCode);
				dbContext.Z80WindingCodes.Update(updatedCode);
				break;
			}
			case WindingCodeType.Pc: {
				var updatedCode = mapper.MapToPc(windingCode);
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
				var windingCode = await _dataContext.Z80WindingCodes.FindAsync(codeId);
				_dataContext.Z80WindingCodes.Remove(windingCode!);
				break;
			}
			case WindingCodeType.Pc: {
				var windingCode = await _dataContext.PcWindingCodes.FindAsync(codeId);
				_dataContext.PcWindingCodes.Remove(windingCode!);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(windingCodeType), windingCodeType, null);
		}
	}
	private bool WindingCodeExists(int codeId, WindingCodeType windingCodeType) {
		return windingCodeType switch {
			WindingCodeType.Z80 => Z80WindingCodes.Any(e => e.Id == codeId),
			WindingCodeType.Pc => PcWindingCodes.Any(e => e.Id == codeId),
			_ => false,
		};
	}
	#endregion

	#region WindingStop Tracking
	public async Task UpdateCurrentWindingStop(int codeId, WindingCodeType windingCodeType, string? ip) {
		string? clientIp = ip ?? HubExtensions.GetConnectionIp(Context);
		_currentWindingStop = await GetWindingCode(codeId, windingCodeType);
		if (_currentWindingStop is null) {
			_logger.LogWarning("Could not find WindingCode with Id {CodeId} and Type {WindingCodeType}", codeId, windingCodeType);
			return;
		}
		if (clientIp is null) {
			_logger.LogWarning("Could not get client IP");
			return;
		}
		_logger.LogInformation("Notifying client {ClientIp} of new WindingStop {WindingStop}", clientIp, _currentWindingStop.Name);
		await Clients.Group(clientIp).CurrentWindingStopUpdated(_currentWindingStop);
	}

	public Task<WindingCode?> GetCurrentWindingStop() => Task.FromResult(_currentWindingStop);
	#endregion
}
