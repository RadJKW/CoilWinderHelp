using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Shared.Models;
namespace MudBlazorPWA.Client.Pages.Employee.AutoDocs;
public partial class AutoDocs : IDisposable {
	[Parameter] public int? WindingCodeId { get; set; }
	private WindingCode? _currentWindingStop;

	public string? PdfUrl { get; private set; }
	public string? VideoUrl { get; set; }
	public List<string> RefMediaContent { get; private set; } = new();
	private double _startWidth = 65;
	public event Action? ReloadSecondaryContent;

	private SecondaryContent _secondaryContent = default!;

	private IJSObjectReference? _moduleJS;

	private readonly DialogOptions _dialogOptions = new() {
		NoHeader = true,
		MaxWidth = MaxWidth.ExtraSmall,
		FullWidth = true,
		Position = DialogPosition.Center,
		CloseOnEscapeKey = false,
		DisableBackdropClick = true
	};


	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		_currentWindingStop = await DirectoryHubClient.GetCurrentCoilWinderStop();
			DirectoryHubClient.CurrentWindingStopUpdated += OnCurrentWindingStopUpdated;
			if (OperatorState.CurrentEmployee is null)
				await AuthenticateUser();
	}
	private async Task AuthenticateUser() {
		var dialog = await DialogService.ShowAsync<EmployeeLoginDialog>(title: string.Empty, options: _dialogOptions);
		var result = await dialog.Result;
		if (!result.Canceled) {
			if ((bool)result.Data && _currentWindingStop is not null) {
				// get a clone of the _currentWindingStop if not null
				// set the _currentWindingStop to null
				// Call OnCurrentWindingStopUpdated with the clone
				var windingStop = _currentWindingStop.Clone();
				Console.WriteLine($"OnParametersSetAsync: {windingStop.Code} | Result: {(bool)result.Data}");
				_currentWindingStop = null;
				OnCurrentWindingStopUpdated(windingStop);
			}
		}
	}
	protected override async Task OnParametersSetAsync() {
		await base.OnParametersSetAsync();
		if (WindingCodeId.HasValue) {
			DirectoryHubClient.SetCurrentCoilWinderStop(WindingCodeId.Value);
		}

	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender) {
			_moduleJS = null;
		}
	}
	protected override bool ShouldRender() {
		// only render this component if the current employee is not null
		return OperatorState.CurrentEmployee is not null;
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
	  //Console.WriteLine(JsonSerializer.Serialize(_currentWindingStop, new JsonSerializerOptions { WriteIndented = true }));
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


	void IDisposable.Dispose() {
		DirectoryHubClient.CurrentWindingStopUpdated -= OnCurrentWindingStopUpdated;
		GC.SuppressFinalize(this);
	}
}
