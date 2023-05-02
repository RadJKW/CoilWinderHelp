using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Client.Services;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AdminDashboard : IDisposable {
	[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
	[Inject] private HubClientService HubClientService { get; set; } = default!;
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private NavigationManager NavigationManager { get; set; } = default!;


	//TODO: use a Dictionary to Track/Revert changes
	private MudDropContainer<DropItem> _dropContainer = default!;
	private readonly List<DropItem> _dropItems = new();
	private readonly List<WindingCode> _windingCodesList = new();
	public Action<string, DropItemAction>? OnDrop { get; set; }
	#region LifeCycle Methods
	protected override async Task OnInitializedAsync() {
		HubClientService.WindingCodeTypeChanged += OnWindingCodeTypeChanged;
		await base.OnInitializedAsync();
		var windingCodes = await HubClientService.GetWindingCodes();
		_windingCodesList.AddRange(windingCodes);
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender) {
			await JSRuntime.InvokeVoidAsync("createCustomTooltip");
		}
	}
	void IDisposable.Dispose() {
		Snackbar.Dispose();
		HubClientService.WindingCodeTypeChanged -= OnWindingCodeTypeChanged;
		GC.SuppressFinalize(this);
	}
	#endregion

	#region Event Handlers
	private void ItemUpdated(MudItemDropInfo<DropItem> dropInfo) {
		var originalItem = _dropItems.FirstOrDefault(d => d.DropZoneId == dropInfo.Item!.DropZoneId && !d.IsCopy);
		var targetDropZone = dropInfo.DropzoneIdentifier;
		if (targetDropZone == "trash" && dropInfo.Item!.IsCopy) {
			_dropItems.Remove(dropInfo.Item);
			OnDrop?.Invoke(dropInfo.Item.DropZoneId, DropItemAction.Removed);
			return;
		}
		if (originalItem is null)
			return;
		if (targetDropZone == originalItem.DropZoneId)
			return;
		// create a copy of the original Item
		var copy = new DropItem {
			DropZoneId = targetDropZone,
			Name = originalItem.Name,
			Path = originalItem.Path,
			IsCopy = true
		};
		// add the copy to the dropItems list
		_dropItems.Add(copy);
		OnDrop?.Invoke(copy.DropZoneId, DropItemAction.Added);
	}

	/// <summary>
	/// Refreshes the list of winding codes when the user changes the winding code type. ( Z80 or PC )
	/// </summary>
	private async void OnWindingCodeTypeChanged() {
		_windingCodesList.Clear();
		var windingCodes = await HubClientService.GetWindingCodes();
		_windingCodesList.AddRange(windingCodes);
		await InvokeAsync(StateHasChanged);
	}
	private void DropItemsUpdated(IEnumerable<DropItem> arg) {
		_dropItems.Clear();
		_dropItems.AddRange(arg);
		StateHasChanged();
	}
	#endregion

	#region Methods
	private Dictionary<string, object> GetDropItemAttributes(DropItem context) {
		// ReSharper disable once UseObjectOrCollectionInitializer
		var attributes = new Dictionary<string, object>();

		attributes.Add("id", "drop-zone-chip");
		attributes.Add("data-name", context.Name);
		attributes.Add("data-path", context.Path);
		attributes.Add(
		"ondblclick",
		EventCallback
			.Factory.Create<MouseEventArgs>(this, () => OpenFilePreview(context.Path)));

		if (context.Type is DropItemType.Pdf or DropItemType.Video)
			attributes.Add("data-url", $"{NavigationManager.BaseUri}files/{context.Path}");

		var copies = _dropItems.Where(d => d.Name == context.Name && d.IsCopy).ToList();
		if (!copies.Any())
			return attributes;
		attributes.Add("data-title", string.Join("<br />", copies.Select(c => c.DropZoneId)));

		return attributes;
	}

	private async Task OpenFilePreview(string? filePath) {
		if (string.IsNullOrEmpty(filePath))
			return;

		var url = $"{NavigationManager.BaseUri}files/{filePath}";
		await JSRuntime.InvokeVoidAsync("openFilePreview", url);
	}
	private void DropItemRemoved(DropItem arg) {
		_dropItems.Remove(arg);
		OnDrop?.Invoke(arg.DropZoneId, DropItemAction.Removed);
	}
	#endregion

	public enum DropItemAction {
		Added,
		Updated,
		Removed,
	}
}
