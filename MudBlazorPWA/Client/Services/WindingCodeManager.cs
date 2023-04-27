using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class WindingCodeManager {
	private readonly HubClientService _directoryHub;
	public IEnumerable<WindingCode> WindingCodes { get; set; } = new List<WindingCode>();
	public WindingCode? SelectedWindingCode { get; set; }

	public WindingCodeManager(HubClientService directoryHub) {
		_directoryHub = directoryHub;
		OnInitialized();
	}

	private async void OnInitialized() {
		WindingCodes = await _directoryHub.GetWindingCodes();
	}
	public async Task FetchWindingCodes() {
		WindingCodes = await _directoryHub.GetWindingCodes();
	}
}
