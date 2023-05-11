using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazorPWA.Shared.Models;
using System.Text.Json;
namespace MudBlazorPWA.Client.Pages.AutoDocs;
public partial class AutoDocs {
	[Parameter]
	public int? WindingCodeId { get; set; }
	private WindingCode? _currentWindingStop;

	public string? PdfUrl { get; private set; }
	public string? VideoUrl { get; set; }
	public List<string> RefMediaContent { get; private set; } = new();
	private double _startWidth = 65;
	public event Action? ReloadSecondaryContent;

	private SecondaryContent _secondaryContent = default!;

	private IJSObjectReference? _moduleJS;


	protected override async Task OnInitializedAsync() {
		_currentWindingStop = await DirectoryHubClient.GetCurrentCoilWinderStop();
		DirectoryHubClient.CurrentWindingStopUpdated += OnCurrentWindingStopUpdated;
		await base.OnInitializedAsync();
	}
	protected override async Task OnParametersSetAsync() {
		if (WindingCodeId.HasValue) {
			DirectoryHubClient.SetCurrentCoilWinderStop(WindingCodeId.Value);
		}
		await base.OnParametersSetAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		if (firstRender) {
			// _moduleJS = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/AutoDocs/AutoDocs.razor.js");
			_moduleJS = null;
		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private void OnCurrentWindingStopUpdated(WindingCode windingCode) {
		Console.WriteLine("OnCurrentWindingStopUpdated: " + windingCode.Code);
		if (_currentWindingStop?.Code == windingCode.Code) {
			Console.WriteLine("OnCurrentWindingStopUpdated: same winding code");
			return;
		}

		if (string.IsNullOrEmpty(windingCode.Media.Pdf)
			&& string.IsNullOrEmpty(windingCode.Media.Video)
			&& windingCode.Media.RefMedia == null) {
			Console.WriteLine("OnCurrentWindingStopUpdated: all urls are null");
			return;
		}
		// if the windingStop is not null , set it to null and update the UI
		if (_currentWindingStop != null) {
			_currentWindingStop = null;
			StateHasChanged();
		}
		// if all of the urls are null, then dont change the _currentWindingStop
		_currentWindingStop = windingCode;
		PdfUrl = windingCode.Media.Pdf;
		VideoUrl = windingCode.Media.Video;
		RefMediaContent = windingCode.Media.RefMedia ?? new();
		StateHasChanged();
		if (_moduleJS != null)
			InvokeAsync(async () => { await _moduleJS.InvokeVoidAsync("init"); });
		Console.WriteLine(JsonSerializer.Serialize(_currentWindingStop, new JsonSerializerOptions { WriteIndented = true }));
	}
	private void HandleDoubleClicked() {
		_startWidth = _startWidth <= 50
			? 70
			: 30;
	}
	private void ToggleContentSize() {
		_startWidth = _startWidth >= 50
			? 40
			: 70;
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
