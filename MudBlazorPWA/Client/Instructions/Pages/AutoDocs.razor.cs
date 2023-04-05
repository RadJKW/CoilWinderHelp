using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AutoDocs
{
	private IWindingCode? _currentWindingStop;
	private double _startWidth = 65;

	private IJSObjectReference? _moduleJS;


	protected override async Task OnInitializedAsync() {
		_currentWindingStop = await DirectoryHubClient.GetCurrentCoilWinderStop();
		DirectoryHubClient.CurrentWindingStopUpdated += OnCurrentWindingStopUpdated;
		await base.OnInitializedAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender) {
			// _moduleJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Instructions/Pages/AutoDocs.razor.js");
			_moduleJS = null;
		}
	}
	private void OnCurrentWindingStopUpdated(object? sender, IWindingCode windingCode) {
		_currentWindingStop = windingCode;
		StateHasChanged();
		if (_moduleJS != null)
			InvokeAsync(async () => { await _moduleJS.InvokeVoidAsync("init"); });
	}
	private void HandleDoubleClicked() {
		_startWidth = _startWidth <= 50 ? 70 : 30;
	}
	private void ToggleContentSize() {
		_startWidth = _startWidth >= 50 ? 40 : 70;
	}
	public async ValueTask DisposeAsync() {
		if (_moduleJS != null)
			await _moduleJS.DisposeAsync();
	}

}
