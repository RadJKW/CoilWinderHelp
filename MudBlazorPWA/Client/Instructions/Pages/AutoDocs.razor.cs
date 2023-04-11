using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Client.Instructions.Components;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AutoDocs
{
	[Parameter]
	public int? WindingCodeId { get; set; }
	private IWindingCode? _currentWindingStop;

	public string? PdfUrl { get; private set; }
	public string? VideoUrl { get; set; }
	public List<string> RefMediaContent { get; private set; } = new();
	private double _startWidth = 65;

	// create an event that child components will subscribe to for when the secondary content is updated
	// this will allow the parent component to reload the secondary content

	public event Action? ReloadSecondaryContent;

	private SecondaryContent _secondaryContent = default!;

	private IJSObjectReference? _moduleJS;


	protected override async Task OnInitializedAsync() {
		_currentWindingStop = await DirectoryHubClient.GetCurrentCoilWinderStop();
		DirectoryHubClient.CurrentWindingStopUpdated += OnCurrentWindingStopUpdated;
		await base.OnInitializedAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		if (firstRender) {
			// _moduleJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Instructions/Pages/AutoDocs.razor.js");
			_moduleJS = null;
			if (WindingCodeId.HasValue) {
				DirectoryHubClient.SetCurrentCoilWinderStop(WindingCodeId.Value);
			}
		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private void OnCurrentWindingStopUpdated(object? sender, IWindingCode windingCode) {
		_currentWindingStop = windingCode;
		PdfUrl = windingCode.Media.Pdf;
		VideoUrl = windingCode.Media.Video;
		RefMediaContent = windingCode.Media.RefMedia ?? new();
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
	public void SecondaryContentUpdated() {
		// dispose and reload SecondaryContent
		ReloadSecondaryContent?.Invoke();
	}
}
