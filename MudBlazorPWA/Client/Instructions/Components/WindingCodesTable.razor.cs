using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Client.Services;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Components;
public partial class WindingCodesTable
{
	[Parameter] [EditorRequired] public required List<IWindingCode> WindingCodes { get; set; }
	[Parameter] [EditorRequired] public required List<DropItem> DropItems { get; set; }

	// create an event callback for the parent component to handle for when CodeType is changed



	[Inject] private HubClientService HubClientService { get; set; } = default!;
	[Inject] private NavigationManager NavigationManager { get; set; } = default!;

	[Inject] private IJSRuntime JSRuntime { get; set; } = default!;

	[Inject] private ILogger<WindingCodesTable> Logger { get; set; } = default!;
	[Inject] private ISnackbar Snackbar { get; set; } = default!;



	private List<DropItem> _dropItems = default!;
	private Division _selectedDivision = Division.All;
	private Division SelectedDivision {
		get => _selectedDivision;
		set {
			_selectedDivision = value;
			UpdateGridFilter();
		}
	}
	private MudTooltip _menuTooltip = default!;
	private readonly List<FilterDefinition<IWindingCode>> _dataGridFilter = new();
	private WindingCodeType CurrentWindingCodeType {
		get => HubClientService.WindingCodeType;
		set => HubClientService.WindingCodeType = value;
	}
	private bool _enableEdit;
	private string? _searchString;
	private bool _menuTooltipVisible;
	private bool MenuTooltipVisible {
		get => _menuTooltipVisible;
		set {
			_menuTooltipVisible = value;
			if (value) {
				_ = HideMenuTooltip();
			}
			else {
				_ctsMenuToolTip.Cancel();
				_ctsMenuToolTip = new();
			}
		}
	}
	private IWindingCode? SelectedWindingCode { get; set; }
	private CancellationTokenSource _ctsMenuToolTip = new();

	protected override Task OnInitializedAsync() {
		HubClientService.WindingCodesDbUpdated += async () => await OnWindingCodesDbUpdated();
		SelectedDivision = Division.D1;
		UpdateGridFilter();
		BuildColumnMap();
		return base.OnInitializedAsync();
	}

	protected override Task OnParametersSetAsync() {
		_dropItems = DropItems;
		return Task.CompletedTask;
	}



	#region DataGrid Methods
	private static bool AssignedMediaDisabled(IWindingCode windingCode) {
		return windingCode.FolderPath == null;
	}
	private async Task RefreshWindingCodes() {
		var windingCodesList = await HubClientService.GetCodeList();
		// replace the list of WindingCodes with the new list
		WindingCodes.Clear();
		WindingCodes.AddRange(windingCodesList);
	}
	private void StartedEditingItem(IWindingCode item) {
		Snackbar.Add($"Started editing, Data = {JsonSerializer.Serialize(item)}", Severity.Info);
	}
	private void CanceledEditingItem(IWindingCode item) {
		Snackbar.Add($"Canceled editing, Data = {JsonSerializer.Serialize(item)}", Severity.Info);
	}
	private void UpdateGridFilter() {
		_dataGridFilter.Clear();
		if (SelectedDivision != Division.All) {
			_dataGridFilter.Add(new() {
				FilterFunction = x => x.Division == SelectedDivision
			});
		}
	}
	#endregion

	private async Task CommitItemChanges(IWindingCode item) {
		bool result = await HubClientService.UpdateWindingCodeDb(item);
		if (!result) {
			Snackbar.Add($"Failed to commit changes, Data = {JsonSerializer.Serialize(item)}", Severity.Error);
			return;
		}
		Snackbar.Add($"Committed changes, Data = {JsonSerializer.Serialize(item)}", Severity.Success);
		IWindingCode? updatedItem = await HubClientService.GetWindingCode(item.Id);
		if (updatedItem != null) {
			int index = WindingCodes.FindIndex(x => x.Id == updatedItem.Id);
			WindingCodes[index] = updatedItem;
			StateHasChanged();
		}
	}

	#region QuickFilterSearch
	private Dictionary<string, Func<IWindingCode, string>> _columnMap = new();

	private void BuildColumnMap() {
		_columnMap = new() {
			["CodeType"] = x => x.CodeTypeId.ToString(),
			["CodeTypeId"] = x => x.CodeTypeId.ToString(),
			["Code"] = x => x.Code,
			["Name"] = x => x.Name,
			["Division"] = x => x.Division.ToString(),
			["Dept"] = x => x.Division.ToString()
		};
	}
	private bool DataGridQuickFilter(IWindingCode x) {
		if (string.IsNullOrWhiteSpace(_searchString)) {
			return true;
		}

		var searchTerms = _searchString.Split(',')
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s));

		foreach (string searchTerm in searchTerms) {
			int delimiterIndex = searchTerm.IndexOfAny(new[] {
				'=', ':'
			});

			if (delimiterIndex < 0) {
				bool matches = _columnMap.Values.Any(propertySelector =>
					propertySelector(x).Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
				if (!matches) {
					return false;
				}
			}
			else {
				string columnName = searchTerm[..delimiterIndex].Trim();
				string searchTermValue = searchTerm[(delimiterIndex + 1)..].Trim();

				// Ensuring case-insensitive column name comparison
				var columnEntry = _columnMap.FirstOrDefault(entry =>
					entry.Key.Equals(columnName, StringComparison.OrdinalIgnoreCase));

				if (columnEntry.Key == null) {
					continue;
				}

				string columnValue = columnEntry.Value(x);
				if (!columnValue.Contains(searchTermValue, StringComparison.OrdinalIgnoreCase)) {
					return false;
				}
			}
		}

		return true;
	}
	#endregion

	private async Task OnWindingCodesDbUpdated() {
		Snackbar.Add("Winding Codes Database Updated", Severity.Success);
		await RefreshWindingCodes();
	}

	private async Task HideMenuTooltip() {
		try {
			await Task.Delay((int)_menuTooltip.UserAttributes["duration"], _ctsMenuToolTip.Token);
		}
		catch (TaskCanceledException) {
			return;
		}
		MenuTooltipVisible = false;
		StateHasChanged();
	}
}
