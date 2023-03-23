using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Hubs;


namespace MudBlazorPWA.Shared.Services;
public enum HubServers
{
	DirectoryHub,
	ChatHub
}
public sealed class HubClientService: IAsyncDisposable
{
	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;
	public event Action? WindingCodesDbUpdated;
	public event EventHandler<WindingCode>? CurrentWindingStopUpdated;
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
	public HubConnection DirectoryHub { get; private set; } = null!;
	private HubConnection ChatHub { get; set; } = null!;

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
		DirectoryHub = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithUrl(_navigationManager.ToAbsoluteUri("/directoryHub"))
			.Build();

		RegisterHubEvents(DirectoryHub);
		await DirectoryHub.StartAsync();
		await DirectoryHub.InvokeAsync<string>("GetServerWindingDocsFolder");
	}
	private void InitializeChatHub() {
		ChatHub = new HubConnectionBuilder()
			.WithAutomaticReconnect()
			.WithUrl(_navigationManager.ToAbsoluteUri("/chatHub"))
			.Build();

		ChatHub.On<string, string>(nameof(IChatHub.NewMessage), (user, message) => {
			NewChatMessage?.Invoke(user, message);
		});

		ChatHub.StartAsync();
	}
	private void RegisterHubEvents(HubConnection hubConnection) {
		hubConnection.On<string[]>("ReceiveAllFolders", folders => {
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
	public async Task<IEnumerable<string>> GetFoldersInPath(string? path = null) {
		var folders = await DirectoryHub.InvokeAsync<IEnumerable<string>>("GetAllFolders", path);
		return folders;
	}

	public async Task<(List<string>, List<string>)> ListMediaFiles(string? path = null) {
		var pdfFiles = await DirectoryHub.InvokeAsync<List<string>>("ListPdfFiles", path);
		var videoFiles = await DirectoryHub.InvokeAsync<List<string>>("ListVideoFiles", path);
		return (pdfFiles, videoFiles);
	}
	public async Task<List<string>> ListPdfFiles(string? path = null) {
		var files = await DirectoryHub.InvokeAsync<List<string>>("ListPdfFiles", path);
		return files;
	}

	public async Task<List<string>> ListVideoFiles(string? path = null)
	{
		var files = await DirectoryHub.InvokeAsync<List<string>>("ListVideoFiles", path);
		return files;
	}
	#endregion

	public async Task<WindingCode?> GetCurrentCoilWinderStop() {
		return await DirectoryHub.InvokeAsync<WindingCode>("GetCurrentWindingStop");
	}
	public async Task SendChatMessage(string user, string message) {
		await ChatHub.InvokeAsync(HubInfo.Actions.SendMessage, user, message, null);
	}
	public async void SetCurrentCoilWinderStop(WindingCode code) {
		await DirectoryHub.InvokeAsync("UpdateCurrentWindingStop", code);
	}
	private void ParseWindingCodeMedia(WindingCode code) {
		if (code.Media.Video != null)
			code.Media.Video = FileServerUrl + code.Media.Video;
		if (code.Media.Pdf != null)
			code.Media.Pdf = FileServerUrl + code.Media.Pdf;
		if (code.Media.ReferenceFolder != null)
			code.Media.ReferenceFolder = FileServerUrl + code.Media.ReferenceFolder;

		CurrentWindingStopUpdated?.Invoke(this, code);
	}

	#region WindingCodeDB Crud
	public async Task<IEnumerable<WindingCode>?> GetCodeList(Division? division = null) {
		var windingCodesList = await DirectoryHub.InvokeAsync<List<WindingCode>?>("GetWindingCodes", division);
		return windingCodesList ?? null;
	}
	public async Task<WindingCode?> GetWindingCode(int codeId) {
		return await DirectoryHub.InvokeAsync<WindingCode>("GetWindingCode", codeId);
	}
	public async Task<bool> AddWindingCode(WindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("CreateWindingCode", code);
	}
	public async Task<bool> UpdateWindingCode(WindingCode code) {
		return await DirectoryHub.InvokeAsync<bool>("UpdateWindingCode", code);
	}
	public async Task<WindingCode?> DeleteWindingCode(WindingCode code) {
		return await DirectoryHub.InvokeAsync<WindingCode>("DeleteWindingCode", code);
	}
	#endregion

	public async ValueTask DisposeAsync() {
		if (DirectoryHub.State == HubConnectionState.Connected)
			await DirectoryHub.DisposeAsync();
	}
}
