using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Models;
using MudBlazorPWA.Shared.Hubs;


namespace MudBlazorPWA.Shared.Services;
public enum HubServers
{
	DirectoryHub,
	ChatHub
}

public class HubClientService
{
	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;
	public event Action? WindingCodesDbUpdated;

	public event Action<List<WindingCode>>? WindingCodesListUpdated;

	public event EventHandler<WindingCode>? CurrentWindingStopUpdated;

	public event Action<string, string>? NewChatMessage;



	public HubClientService(NavigationManager navigationManager) {
		_navigationManager = navigationManager;
		InitializeDirectoryHub();
		InitializeChatHub();
		FileServerUrl = _navigationManager
			.ToAbsoluteUri("/files/");
	}

	private Uri? FileServerUrl { get; init; }
	public HubConnection DirectoryHub { get; private set; } = null!;
	private HubConnection ChatHub { get; set; } = null!;
	private readonly NavigationManager _navigationManager;

		private async void InitializeDirectoryHub() {
		DirectoryHub = new HubConnectionBuilder()
			.WithUrl(_navigationManager.ToAbsoluteUri("/directoryHub"))
			.Build();

		RegisterHubEvents(DirectoryHub);
		await DirectoryHub.StartAsync();
		await DirectoryHub.InvokeAsync<string>("GetServerWindingDocsFolder");
	}

	private void InitializeChatHub() {
		ChatHub = new HubConnectionBuilder()
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

	public async Task SendChatMessage(string user, string message) {
		await ChatHub.InvokeAsync(HubInfo.Actions.SendMessage, user, message, null);

	}
	public async void GetCodeList() {
		var windingCodesList = await DirectoryHub.InvokeAsync<List<WindingCode>?>("GetWindingCodes", Division.D1);
		if (windingCodesList != null)
			WindingCodesListUpdated?.Invoke( windingCodesList);

	}
	public async void SetCurrentCoilWinderStop(WindingCode code) {
		await DirectoryHub.InvokeAsync("UpdateCurrentWindingStop", code);
	}

	public async Task<WindingCode?> GetCurrentCoilWinderStop() {
		return await DirectoryHub.InvokeAsync<WindingCode>("GetCurrentWindingStop");
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

	public  HubConnection GetHubConnection(HubServers hubServer) {
		return hubServer switch {
			HubServers.DirectoryHub => DirectoryHub,
			HubServers.ChatHub => ChatHub,
			_ => throw new ArgumentOutOfRangeException(nameof(hubServer), hubServer, null)
		};
	}
}
