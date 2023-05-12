using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazorPWA.Shared.Models;
using System.Text.Json;
namespace MudBlazorPWA.Client.Pages.Admin.StopsEditor;
public partial class StopDataGrid : IDisposable {
	[Parameter] [EditorRequired] public required List<WindingCode> WindingCodes { get; set; }

	// create an event callback for the parent component to handle for when CodeType is changed

	[Inject] private ILogger<StopDataGrid> Logger { get; set; } = default!;
	[Inject] private ISnackbar Snackbar { get; set; } = default!;


	private readonly List<FilterDefinition<WindingCode>> _dataGridFilter = new();
  #region Properties
	private Division _selectedDivision = Division.All;
	private Division SelectedDivision {
		get => _selectedDivision;
		set {
			_selectedDivision = value;
			UpdateGridFilter();
		}
	}
	private MudTooltip _menuTooltip = default!;
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
	private CancellationTokenSource _ctsMenuToolTip = new();
	#endregion

	#region Lifecycle
	protected override Task OnInitializedAsync() {
		State.StateChanged += OnStateChanged;
		SelectedDivision = Division.D1;
		UpdateGridFilter();
		BuildColumnMap();
		return base.OnInitializedAsync();
	}
	private void OnStateChanged() => StateHasChanged();

	public void Dispose() {
		State.StateChanged -= OnStateChanged;
		GC.SuppressFinalize(this);
	}
	#endregion

	#region DataGrid Methods
	private void UpdateGridFilter() {
		_dataGridFilter.Clear();
		if (SelectedDivision != Division.All) {
			_dataGridFilter.Add(
			new() {
				FilterFunction = x => x.Division == SelectedDivision
			});
		}
	}
	#endregion

  #region Methods
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
	private async Task CommitItemChanges(WindingCode item) {

		// add the item to the State.ModifiedWindingCodes list
		// use State.ModifyWindingCode(item) to add the item to the list

		bool result = await State.ModifyWindingCode(item);
		if (!result) {
			Snackbar.Add($"Failed to commit changes, Data = {JsonSerializer.Serialize(item)}", Severity.Error);
			return;
		}
		Snackbar.Add($"Committed changes, Data = {JsonSerializer.Serialize(item)}", Severity.Success);
	}
#endregion

	#region QuickFilterSearch
	private Dictionary<string, Func<WindingCode, string>> _columnMap = new();

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
	private bool DataGridQuickFilter(WindingCode x) {
		if (string.IsNullOrWhiteSpace(_searchString)) {
			return true;
		}

		var searchTerms = _searchString.Split(',')
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s));

		foreach (string searchTerm in searchTerms) {
			int delimiterIndex = searchTerm.IndexOfAny(
			new[] {
				'=', ':'
			});

			if (delimiterIndex < 0) {
				bool matches = _columnMap.Values.Any(
				propertySelector =>
					propertySelector(x).Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
				if (!matches) {
					return false;
				}
			}
			else {
				string columnName = searchTerm[..delimiterIndex].Trim();
				string searchTermValue = searchTerm[(delimiterIndex + 1)..].Trim();

				// Ensuring case-insensitive column name comparison
				var columnEntry = _columnMap.FirstOrDefault(
				entry =>
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

	private string RowStyleFunc(WindingCode arg1, int arg2) {
		return arg1 == State.SelectedWindingCode
			? "background-color: rgb(40 70 44 / 49%);box-shadow: var(--mud-elevation-7);"
			: string.Empty;
	}
}
