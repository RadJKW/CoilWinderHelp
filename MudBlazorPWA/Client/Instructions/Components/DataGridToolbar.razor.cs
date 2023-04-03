using Microsoft.AspNetCore.Components;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Components;
public partial class DataGridToolbar : ComponentBase
{
	[Parameter] public bool EnableEdit { get; set; }
	[Parameter] public string? SearchString { get; set; }
	[Parameter] public List<Division> SelectedDivisions { get; set; } = new List<Division>();
	[Parameter] public EventCallback<(bool, Division)> OnDivisionToggled { get; set; }
	[Parameter] public EventCallback<bool> EnableEditChanged { get; set; }

	private async Task OnDivisionCheckedChanged(bool isChecked, Division division) {
		await OnDivisionToggled.InvokeAsync((isChecked, division));
	}

	protected override async Task OnInitializedAsync() {
		 await base.OnInitializedAsync();

		 if (EnableEditChanged.HasDelegate)
			 _ = EnableEditChanged.InvokeAsync(EnableEdit);
	}
}
