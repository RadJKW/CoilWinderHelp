using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Shared.Services;

public class HubClientService
{
	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;
	public event Action? WindingCodesDbUpdated;

	public event EventHandler<WindingCode>? CurrentWindingStopUpdated;

	public HubClientService(NavigationManager navigationManager) {
		_navigationManager = navigationManager;
		InitializeDirectoryHub();
	}
	public HubConnection Hub { get; private set; } = null!;
	private readonly NavigationManager _navigationManager;

	private void InitializeDirectoryHub() {

		Hub = new HubConnectionBuilder()
			.WithUrl(_navigationManager.ToAbsoluteUri("/directoryHub"))
			.Build();

		RegisterHubEvents(Hub);
		Hub.StartAsync();
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
		hubConnection.On<WindingCode>("CurrentWindingStopUpdated", code => {
			CurrentWindingStopUpdated?.Invoke(this ,code);
		});

	}
	public async void SetCurrentCoilWinderStop(WindingCode code) {
		await Hub.InvokeAsync("UpdateCurrentWindingStop", code);

	}

	public async Task<WindingCode?> GetCurrentCoilWinderStop() {
	 return await Hub.InvokeAsync<WindingCode>("GetCurrentWindingStop");
	}

	public HubConnection GetHubConnection() {
		return Hub;
	}
}
