using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace MudBlazorPWA.Shared.Services;

public class HubClientService
{
	public HubConnection? Hub;
	private readonly NavigationManager _navigationManager;

	public event Action<string[]>? ReceiveAllFolders;
	public event Action<string, string[]?, string[]?>? ReceiveFolderContent;

	public event Action? WindingCodesDbUpdated;


	public HubClientService(NavigationManager navigationManager) {
		_navigationManager = navigationManager;
		// establish the connection to the hub on the server

		InitializeDirectoryHub();

	}

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
	}

	public async Task InvokeAsync(string methodName, params object[]? args) {
		if (Hub is null) {
			InitializeDirectoryHub();
		}
		if (Hub != null)
			await Hub.InvokeAsync(methodName, args);
	}
}
