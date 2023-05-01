using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Services;
public class WindingCodeManager {
	public event Action<IEnumerable<WindingCode>>? WindingCodesChanged;

	private readonly HubClientService _directoryHub;
	private IEnumerable<WindingCode> _windingCodes = new List<WindingCode>();
	public WindingCode? SelectedWindingCode { get; set; }

	public WindingCodeManager(HubClientService directoryHub) {
		_directoryHub = directoryHub;
		OnInitialized();
	}

	private async void OnInitialized() {
		_directoryHub.WindingCodeTypeChanged += async () => await FetchWindingCodes();
		_directoryHub.WindingCodesDbUpdated += async () => await FetchWindingCodes();
		WindingCodes = await _directoryHub.GetWindingCodes();
	}

	public IEnumerable<WindingCode> WindingCodes {
		get => _windingCodes;
		private set {
			_windingCodes = value;
			WindingCodesChanged?.Invoke(_windingCodes);
		}
	}
	public async Task<WindingCode?> FetchWindingCode(int id) {
		return await _directoryHub.GetWindingCode(id);
	}
	private async Task FetchWindingCodes() {
		WindingCodes = await _directoryHub.GetWindingCodes();
	}
	public async Task<bool> UpdateWindingCode(WindingCode windingCode) {
		return await _directoryHub.UpdateWindingCodeDb(windingCode);
	}
}
