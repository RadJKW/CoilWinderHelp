using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Extensions;
using MudBlazorPWA.Shared.Interfaces;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Services;
public enum HubServers {
	DirectoryHub,
	ChatHub
}

public class HubClientService {
	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;
	public event Action? WindingCodesDbUpdated;
	public event Action<WindingCode>? CurrentWindingStopUpdated;

	public event Action? WindingCodeTypeChanged;
	public event Action<string, string>? NewChatMessage;
	public HubClientService(NavigationManager navigationManager, ILogger<HubClientService> logger) {
		_navigationManager = navigationManager;
		_logger = logger;
		InitializeDirectoryHub();
		InitializeChatHub();
		FileServerUrl = _navigationManager
			.ToAbsoluteUri("/files/");
		GetServerDocsFolder();
	}

	// ReSharper disable once NotAccessedField.Local
	private readonly ILogger<HubClientService> _logger;
	private readonly NavigationManager _navigationManager;
	private Uri? FileServerUrl { get; init; }
	private HubConnection DirectoryHub { get; set; } = null!;
	private HubConnection ChatHub { get; set; } = null!;

	private WindingCodeType _windingCodeType = WindingCodeType.Z80;
	public WindingCodeType WindingCodeType {
		get => _windingCodeType;
		set {
			_windingCodeType = value;
			WindingCodeTypeChanged?.Invoke();
		}
	}
	public string WindingDocsFolder { get; private set; } = string.Empty;
// TODO: GetHubConnection is redundant, remove it
	public HubConnection GetHubConnection(HubServers hubServer) {
		return hubServer switch {
			HubServers.DirectoryHub => DirectoryHub,
			HubServers.ChatHub => ChatHub,
			_ => throw new ArgumentOutOfRangeException(nameof(hubServer), hubServer, null)
		};
	}


	private async void GetServerDocsFolder() {
		WindingDocsFolder = await DirectoryHub.InvokeAsync<string>("GetServerWindingDocsFolder");
	}
	private async void InitializeDirectoryHub() {
		var hubUrl = _navigationManager.ToAbsoluteUri("/directoryHub");
		DirectoryHub = new HubConnectionBuilder()
			.WithUrl(hubUrl)
			.WithAutomaticReconnect()
			.AddJsonProtocol(
			options => {
				options.PayloadSerializerOptions.Converters.Add(new WindingCodeJsonConverter());
			})
			.Build();

		RegisterHubEvents(DirectoryHub);
		await DirectoryHub.StartAsync();
	}
	private void InitializeChatHub() {
		ChatHub = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithUrl(_navigationManager.ToAbsoluteUri("/chatHub"))
			.Build();

		ChatHub.On<string, string>(
		nameof(IChatHub.NewMessage),
		(user, message) => {
			NewChatMessage?.Invoke(user, message);
		});

		ChatHub.StartAsync();
	}
	private void RegisterHubEvents(HubConnection hubConnection) {
		hubConnection.On<string[]>(
		"ReceiveAllFolders",
		folders => {
			ReceiveAllFolders?.Invoke(folders);
		});
		hubConnection.On<string, string[]?, string[]?>(
		"ReceiveFolderContent",
		(path, files, folders) => {
			ReceiveFolderContent?.Invoke(path, files, folders);
		});
		hubConnection.On("WindingCodesDbUpdated", () => { WindingCodesDbUpdated?.Invoke(); });
		hubConnection.On<WindingCode>("CurrentWindingStopUpdated", ParseWindingCodeMedia);
	}

	#region DirectoryInfo
	public async Task<DirectoryNode> GetDirectorySnapshot() {
		var directory = await DirectoryHub.InvokeAsync<DirectoryNode>("BuildDirectorySnapshot", null);
		return directory;
	}
	#endregion


	public async Task<WindingCode?> GetCurrentCoilWinderStop() {
		return await DirectoryHub.InvokeAsync<WindingCode>("GetCurrentWindingStop");
	}
	public async Task SendChatMessage(string user, string message) {
		await ChatHub.InvokeAsync("SendMessage", user, message, null);
	}
	public async void SetCurrentCoilWinderStop(int id, string? clientIp = null) {
		await DirectoryHub.InvokeAsync("UpdateCurrentWindingStop", id, WindingCodeType, clientIp);
	}
	private void ParseWindingCodeMedia(WindingCode code) {
		Console.WriteLine($"ParseWindingCodeMedia: {code.Code}");
		if (code.Media.Video != null)
			code.Media.Video = FileServerUrl + code.Media.Video;
		if (code.Media.Pdf != null)
			code.Media.Pdf = FileServerUrl + code.Media.Pdf;
		// append 'FileServerUrl' to each item in code.Media.RefMedia list
		if (code.Media.RefMedia != null && code.Media.RefMedia.Any())
			// iterate through each item in the list
			for (var i = 0; i < code.Media.RefMedia.Count; i++)
				// append 'FileServerUrl' to each item in the list
				code.Media.RefMedia[i] = FileServerUrl + code.Media.RefMedia[i];

		//Console.WriteLine(JsonSerializer.Serialize(code, new JsonSerializerOptions { WriteIndented = true }));

		CurrentWindingStopUpdated?.Invoke(code);
	}

	#region WindingCodeDB Crud
	public async Task<IEnumerable<WindingCode>> GetWindingCodes(Division? division = null) {
		var windingCodesList = await DirectoryHub.InvokeAsync<List<WindingCode>?>("GetWindingCodes", division, WindingCodeType);
		return windingCodesList ?? new List<WindingCode>();
	}
	public async Task<WindingCode?> GetWindingCode(int codeId) {
		return await DirectoryHub.InvokeAsync<WindingCode>("GetWindingCode", codeId, WindingCodeType);
	}

	public async Task<List<WindingCode>> GetWindingCodesForImport(WindingCode code) {
		var divisions = new List<Division>() { Division.D1, Division.D2, Division.D3 };

		divisions.Remove(code.Division);
		var windingCodes = await GetWindingCodes();
		var windingCodesForImport = windingCodes.Where(wc => wc.Code == code.Code && divisions.Contains(wc.Division)).ToList();
		return windingCodesForImport;
	}
	public async Task<bool> AddWindingCode(WindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("CreateWindingCode", code, WindingCodeType);
	}
	public async Task<bool> UpdateWindingCodeDb(WindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("UpdateWindingCode", code, WindingCodeType);
	}
	public async Task<WindingCode?> DeleteWindingCode(WindingCode code) {
		return await DirectoryHub.InvokeAsync<WindingCode>("DeleteWindingCode", code, WindingCodeType);
	}
	#endregion
}
