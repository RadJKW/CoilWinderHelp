using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazorPWA.Client.ViewModels;
using MudBlazorPWA.Shared.Models;

namespace MudBlazorPWA.Client.Instructions.Pages;
public partial class AdminDashboard
{
	/*private const string DzCodeFolder = "DZ-Code-Folder";
	private const string DzCodePdf = "DZ-Code-Pdf";
	private const string DzCodeVideo = "DZ-Code-Video";
	private const string DzCodeRefMedia = "DZ-Code-Ref";*/
	private MudDropContainer<DropItem> _dropContainer = default!;
	private readonly List<string> _folderPathsCollection = new();
	private readonly List<DropItem> _dropItems = new();
	private readonly List<IWindingCode> _windingCodesList = new();

	protected override async Task OnInitializedAsync() {
		HubClientService.WindingCodeTypeChanged += async () => await OnWindingCodeTypeChanged();
		await base.OnInitializedAsync();
		var getWindingCodesTask = HubClientService.GetCodeList();
		var getFolderPathsTask = HubClientService.GetFoldersInPath();

		await Task.WhenAll(getWindingCodesTask, getFolderPathsTask);

		if (getWindingCodesTask.Result != null) {
			_windingCodesList.AddRange(getWindingCodesTask.Result);
			_folderPathsCollection.AddRange(getFolderPathsTask.Result);
		}
	}
	private async Task OnWindingCodeTypeChanged() {
		_folderPathsCollection.Clear();
		_windingCodesList.Clear();
		await OnInitializedAsync();
		await InvokeAsync(StateHasChanged);
	}
	protected override async Task OnAfterRenderAsync(bool firstRender) {
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender) {
			await JSRuntime.InvokeVoidAsync("createCustomTooltip");
		}
	}
	void IDisposable.Dispose() {
		Snackbar.Dispose();
	}
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
				return;
			case true when targetZoneId != dropItem.OriginalIdentifier:
				dropItem.Identifier = targetZoneId;
				return;
			case true when dropItemListWithCopies.Any(x => x.Identifier == targetZoneId):
				_dropItems.Remove(dropItem);
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
	private Task DropItemsUpdated(List<DropItem> arg) {
		if (_dropItems.Any()) {
			_dropItems.Clear();
		}

		_dropItems.AddRange(arg);
		return Task.CompletedTask;
	}

	/*private async Task OnDropItemsViewChanged(bool arg) {
	  _dropItemsViewDense = arg;
	  await InvokeAsync(StateHasChanged);
	}*/
	private async Task OpenFilePreview(string? filePath) {
		if (string.IsNullOrEmpty(filePath))
			return;

		var url = $"{NavigationManager.BaseUri}files/{filePath}";
		await JSRuntime.InvokeVoidAsync("openFilePreview", url);
	}
}
