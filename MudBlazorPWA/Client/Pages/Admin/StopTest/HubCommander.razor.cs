using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorPWA.Client.Services;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Pages.Admin.StopTest;
public partial class HubCommander {
	private HubConnection? _hubConnection;
	private List<string>? CallbackMethods { get; set; }
	private IEnumerable<IWindingCode>? WindingCodes { get; set; }
	private string? _selectedCallbackMethod = string.Empty;
	private string _selectedGroup = string.Empty;
	private readonly List<string> _connectedClients = new();
	private int _selectedRadio = 99;
	private IWindingCode? _selectedWindingCode;
	private int SelectedRadio {
		get => _selectedRadio;
		set {
			_selectedRadio = value;
			_ = SetHubConnection();
		}
	}

	private bool CheckForValidHubConnection => _hubConnection == null;
	/*&& string.IsNullOrEmpty(_selectedCallbackMethod)
	  && string.IsNullOrEmpty(_selectedGroup);*/


	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
	}

	private async Task SetHubConnection() {
		// clear any of the selected values from callback and clients select
		_selectedCallbackMethod = string.Empty;
		_selectedGroup = string.Empty;
		_selectedWindingCode = null;
		_connectedClients.Clear();

		if (SelectedRadio == 99) {
			_hubConnection = null;
			return;
		}

		_hubConnection = SelectedRadio switch {
			(int)HubServers.ChatHub
				=> DirectoryHub.GetHubConnection(HubServers.ChatHub),
			(int)HubServers.DirectoryHub
				=> DirectoryHub.GetHubConnection(HubServers.DirectoryHub),
			_ => null
		};

		if (_hubConnection == null) {
			return;
		}

		var connectedGroups = await _hubConnection
			.InvokeAsync<List<string>>("GetConnectedClients");

		_connectedClients
			.AddRange(connectedGroups.Distinct());

		// if only one item in the list, set it to the selected group
		if (_connectedClients.Count == 1) {
			_selectedGroup = _connectedClients[0];
		}

		CallbackMethods =
			await _hubConnection
				.InvokeAsync<List<string>>("GetCallbackMethods");

		if (SelectedRadio == (int)HubServers.DirectoryHub) {
			WindingCodes = await DirectoryHub.GetWindingCodes(Division.D1);
			_selectedCallbackMethod = CallbackMethods.Find(x => x.Contains("Update", StringComparison.OrdinalIgnoreCase));
			StateHasChanged();
		}
	}


	protected override async Task OnAfterRenderAsync(bool firstRender) {
		await base.OnAfterRenderAsync(firstRender);

		if (firstRender) {
			await SetHubConnection();
		}
	}
	private bool IsNotifyDisabled => CanNotifyClients();

	private bool CanNotifyClients() {
		return SelectedRadio switch {
			(int)HubServers.ChatHub
				=> string.IsNullOrEmpty(_selectedCallbackMethod)
				   || string.IsNullOrEmpty(_selectedGroup)
				   || _hubConnection == null,
			(int)HubServers.DirectoryHub
				=> string.IsNullOrEmpty(_selectedCallbackMethod)
				   || string.IsNullOrEmpty(_selectedGroup)
				   || _selectedWindingCode == null,
			_
				=> true
		};
	}

	private async Task NotifyClients() {
		switch (SelectedRadio) {
			case (int)HubServers.ChatHub:
				await NotifyChatHub();
				break;
			case (int)HubServers.DirectoryHub:
				await NotifyDirectoryHub();
				break;
		}
	}

	private async Task NotifyChatHub() {
		if (string.IsNullOrEmpty(_selectedCallbackMethod) ||
		    string.IsNullOrEmpty(_selectedGroup) ||
		    _hubConnection == null) {
			return;
		}

		if (!string.IsNullOrEmpty(_selectedGroup)) {
			await _hubConnection
				.InvokeAsync(
				_selectedCallbackMethod,
				"commander",
				"sent from page",
				_selectedGroup);
		}
	}

	private async Task NotifyDirectoryHub() {
		// if _selectedCallbackMethod is null, return.

		if (string.IsNullOrEmpty(_selectedCallbackMethod))
			return;

		if (_selectedCallbackMethod == "UpdateCurrentWindingStop" && _selectedWindingCode != null) {
			DirectoryHub.SetCurrentCoilWinderStop(_selectedWindingCode.Id, _selectedGroup);
			await DirectoryHub.SendChatMessage("commander", "updated current winding stop");
		}

		if (_selectedCallbackMethod == "UpdateCurrentWindingStop" && _selectedWindingCode == null) {
			await DirectoryHub.SendChatMessage("commander", "no winding stop selected");
		}
	}
}
