using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AdminDashboard: IDisposable
{
	private MudDropContainer<DropItem> _dropContainer = default!;
	private readonly List<string> _folderPathsCollection = new();
	private readonly List<DropItem> _dropItems = new();
	private readonly List<IWindingCode> _windingCodesList = new();
	public Action<string?> OnDrop { get; set; } = default!;

	#region LifeCycle Methods
	protected override async Task OnInitializedAsync() {
		HubClientService.WindingCodeTypeChanged += OnWindingCodeTypeChanged;
		await base.OnInitializedAsync();
		var getWindingCodesTask = HubClientService.GetCodeList();
		var getFolderPathsTask = HubClientService.GetFoldersInPath();

		await Task.WhenAll(getWindingCodesTask, getFolderPathsTask);

		if (getWindingCodesTask.Result != null) {
			_windingCodesList.AddRange(getWindingCodesTask.Result);
			_folderPathsCollection.AddRange(getFolderPathsTask.Result);
		}
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
		DropItem? dropItem = dropInfo.Item;
		string? targetZoneId = dropInfo.DropzoneIdentifier;

		var dropItemListWithCopies = _dropItems.Where(x => x.Name == dropItem.Name && x.IsCopy).ToList();


		// If the item can't be dropped, just return
		if (!CanDropItem(dropInfo))
			return;

		if (targetZoneId == dropItem.Identifier)
			return;

		switch (dropItem.IsCopy) {
			case true when targetZoneId == dropItem.OriginalIdentifier:
				_dropItems.Remove(dropItem);
				OnDrop.Invoke(dropItem.Identifier);
				return;
			case true when targetZoneId != dropItem.OriginalIdentifier:
				dropItem.Identifier = targetZoneId;
				OnDrop.Invoke(dropItem.Identifier);
				return;
			case true when dropItemListWithCopies.Any(x => x.Identifier == targetZoneId):
				_dropItems.Remove(dropItem);
				OnDrop.Invoke(dropItem.Identifier);
				return;
			case false when dropItemListWithCopies.Any(x => x.Identifier == targetZoneId):
				return;

			default:
				var dropItemCopy = new DropItem {
					Name = dropItem.Name,
					Path = dropItem.Path,
					Type = dropItem.Type,
					Identifier = dropInfo.DropzoneIdentifier,
					OriginalIdentifier = dropItem.Identifier,
					IsCopy = true
				};
				DropItem? originalDropItem = _dropItems.FirstOrDefault(x => !x.IsCopy && x.Name == dropItem.Name);


				if (originalDropItem != null) {
					_dropItems.Insert(_dropItems.IndexOf(originalDropItem), dropItemCopy);
				}
				else {
					_dropItems.Add(dropItemCopy);
				}
				OnDrop.Invoke(dropItem.Identifier);
				break;
		}
	}
	private bool CanDropItem(MudItemDropInfo<DropItem> dropInfo) {
		DropItem? dropItem = dropInfo.Item;
		string? targetZoneId = dropInfo.DropzoneIdentifier;

		// If the item is going back to its original destination
		if (dropItem.OriginalIdentifier == targetZoneId)
			return true;

		// TODO: Add logic to check if the item can be dropped in the target zone
		//if the target zone is one of DZ-Code-Folder, DZ-Code-Pdf, DZ-Code-Video,
		// and the there is already an item in that zone, delete the existing item


		// If the item is a copy and is being dragged to another list where that item already exists
		return !dropItem.IsCopy || _dropItems.Any(x => x.Name == dropItem.Name && x.Identifier == targetZoneId) != true;
	}
	private async void OnWindingCodeTypeChanged() {
		_folderPathsCollection.Clear();
		_windingCodesList.Clear();
		await OnInitializedAsync();
		await InvokeAsync(StateHasChanged);
	}
	private void DropItemsUpdated(IEnumerable<DropItem> arg) {
		_dropItems.Clear();
		_dropItems.AddRange(arg);
	}
	#endregion

	#region Methods
	private Dictionary<string, object> GetDropItemAttributes(DropItem context) {
		// ReSharper disable once UseObjectOrCollectionInitializer
		var attributes = new Dictionary<string, object>();

		attributes.Add( "id", "drop-zone-chip");
		attributes.Add("data-name", context.Name!);
		attributes.Add("data-path", context.Path!);
		attributes.Add("ondblclick", EventCallback
			.Factory.Create<MouseEventArgs>(this, () => OpenFilePreview(context.Path)));

		if (context.Type is DropItemType.Pdf or DropItemType.Video)
			attributes.Add("data-url", $"{NavigationManager.BaseUri}files/{context.Path}");

		var copies = _dropItems.Where(d => d.Name == context.Name && d.IsCopy).ToList();
		if (!copies.Any())
			return attributes;
		attributes.Add("data-title", string.Join("<br />", copies.Select(c => c.Identifier)));

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
		OnDrop.Invoke(arg.Identifier);
	}
	#endregion

}
